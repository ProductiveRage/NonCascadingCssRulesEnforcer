using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSSMinifier.FileLoaders;
using CSSParser;
using CSSParser.ExtendedLESSParser;
using NonCascadingCSSRulesEnforcer.Rules;

namespace NonCascadingCSSRulesEnforcer.CSSMinifierIntegration
{
	/// <summary>
	/// This class allows the rules validation process to be integrated into the CSS loading that the CSSMinifier performs (see
	/// https://bitbucket.org/DanRoberts/cssminifier repository).
	/// 
	/// This will load content and apply all of the the specified validation rules at the appropriate points (some process individual files,
	/// some process combined - but not compiled - content and some process combined and compiled content). In order to intercept loading
	/// at the required points, both a baseContentLoader (that loads individual files without processing) and a compilerGeneraor (that
	/// wraps a baseContentLoader and applies the required processing to the content) must be specified. The baseContentLoader will
	/// be wrapped so that individual-file rules can be applied but the content will be unaltered.
	/// </summary>
	public class RuleEnforcingCssFileLoader : ITextFileLoader
	{
		private readonly IEnumerable<IEnforceRules> _rules;
		private readonly StyleSheetTypeDeterminer _styleSheetTypeDeterminer;
		private readonly ITextFileLoader _baseContentLoader;
		private readonly CompilerGenerator _compilerGenerator;
		private readonly Action<BrokenRuleEncounteredInFileException> _optionalBrokenFileCallback;
		public RuleEnforcingCssFileLoader(
			IEnumerable<IEnforceRules> rules,
			StyleSheetTypeDeterminer styleSheetTypeDeterminer,
			ITextFileLoader baseContentLoader,
			CompilerGenerator compilerGenerator,
			Action<BrokenRuleEncounteredInFileException> optionalBrokenFileCallback = null)
		{
			if (rules == null)
				throw new ArgumentNullException("rules");
			if (styleSheetTypeDeterminer == null)
				throw new ArgumentNullException("styleSheetTypeDeterminer");
			if (baseContentLoader == null)
				throw new ArgumentNullException("baseContentLoader");
			if (compilerGenerator == null)
				throw new ArgumentNullException("compilerGenerator");

			_rules = rules.ToArray();
			if (_rules.Any(r => r == null))
				throw new ArgumentException("Null encountered in rules set");

			_styleSheetTypeDeterminer = styleSheetTypeDeterminer;
			_baseContentLoader = baseContentLoader;
			_compilerGenerator = compilerGenerator;
			_optionalBrokenFileCallback = optionalBrokenFileCallback;
		}

		/// <summary>
		/// This must return an ITextFileLoader that will load content through the specified ITextFileLoader and process the content
		/// as required. It will never be given a null baseContentLoader reference and must never return a null reference.
		/// </summary>
		public delegate ITextFileLoader CompilerGenerator(ITextFileLoader baseContentLoader);

		/// <summary>
		/// This will never be given a null or blank relativePath, it must always return a valid StyleSheetTypeOptions value (or throw
		/// an exception if unable to determine the appropriate value)
		/// </summary>
		public delegate StyleSheetTypeOptions StyleSheetTypeDeterminer(string relativePath);

		/// <summary>
		/// This will throw a BrokenRuleEncounteredInFileException if any of the specified rules are broken
		/// </summary>
		public TextFileContents Load(string relativePath)
		{
			if (string.IsNullOrWhiteSpace(relativePath))
				throw new ArgumentException("Null/blank relativePath specified");

			// Several passes may be required for apply all of the rules - the first pass is to deal with rules that apply to individual files,
			// this is dealt with by wrapping the specified contentLoader in a CallbackMakingTextFileLoader and applying the rules as each file
			// is loaded. The callback from each Load call is also used to build up a representation of the combined content across all of the
			// files (before any processing or compilation work has been applied to the content). Subsequent passes (further down) will process
			// rules that apply to combined or compiled content.
			var loadedContentBuilder = new StringBuilder();
			var rulesForIndividualFiles = _rules.Where(r =>
				r.DoesThisRuleApplyTo(StyleSheetTypeOptions.Reset) ||
				r.DoesThisRuleApplyTo(StyleSheetTypeOptions.Themes) ||
				r.DoesThisRuleApplyTo(StyleSheetTypeOptions.Other)
			);
			var individualFileValidatingAndTrackingContentLoader = new CallbackMakingTextFileLoader(
				_baseContentLoader,
				content =>
				{
					if (content == null)
						throw new ArgumentNullException("content");

					// Apply the rules for individual files (if any)
					if (rulesForIndividualFiles.Any())
					{
						var individualContentFragments = LessCssHierarchicalParser.ParseIntoStructuredData(
							Parser.ParseLESS(content.Content)
						);
						var styleSheetType = _styleSheetTypeDeterminer(content.RelativePath);
						foreach (var ruleForIndividualFiles in rulesForIndividualFiles)
						{
							if (ruleForIndividualFiles.DoesThisRuleApplyTo(styleSheetType))
							{
								// If there's no callback then we're in the "throw as soon as validation rule broken" mode. If there IS a
								// callback then we want to gather ALL validation rule breaks and pass them to the caller
								if (_optionalBrokenFileCallback == null)
								{
									try
									{
										ruleForIndividualFiles.EnsureRulesAreMet(individualContentFragments);
									}
									catch (BrokenRuleEncounteredException e)
									{
										throw new BrokenRuleEncounteredInFileException(e, styleSheetType, content.RelativePath);
									}
								}
								else
								{
									foreach (var brokenRule in ruleForIndividualFiles.GetAnyBrokenRules(individualContentFragments))
									{
										_optionalBrokenFileCallback(
											new BrokenRuleEncounteredInFileException(brokenRule, styleSheetType, content.RelativePath)
										);
									}
								}
							}
						}
					}

					// Build up the combined content (I don't expect that parallel requests will be going on but considering the amount of
					// processing that will be going on with the rules validation, a tiny overhead from locking here for safety will have
					// no significant negative impact)
					lock (loadedContentBuilder)
					{
						// Using AppendLine rather than Append means that if any files have a single-line comment (eg. "// Comment") on
						// their last line then the content will be correctly interpreted as a comment when the combined content is
						// parsed (otherwise the first line of the next file will end up on the same line as the comment and some
						// of it may be incorrectly identified as comment content)
						loadedContentBuilder.AppendLine(content.Content);
					}
				}
			);

			// Process the content by retrieving an ITextFileLoader through the provided compilerGenerator, using the content loader above
			// that will deal with individual-file rules and generate combined content for use below
			var compiler = _compilerGenerator(individualFileValidatingAndTrackingContentLoader);
			if (compiler == null)
				throw new Exception("The provided compiledGenerator returned null - this is not valid");
			var compiledContent = compiler.Load(relativePath);

			// The source content has all been processed in terms of import flattening, LESS compilation (and the various other actions)
			// but rules that apply to Combined and Compiled content haven't been yet.
			// - If there are any rules that apply to Compiled content then the compiled content is parsed and the data passed through
			//   each applicable rule
			var rulesForCompiledContent = _rules.Where(r => r.DoesThisRuleApplyTo(StyleSheetTypeOptions.Compiled));
			if (rulesForCompiledContent.Any())
			{
				var compiledContentFragments = LessCssHierarchicalParser.ParseIntoStructuredData(
					Parser.ParseLESS(compiledContent.Content)
				);
				foreach (var ruleForCompiledContent in rulesForCompiledContent)
				{

					
					// If there's no callback then we're in the "throw as soon as validation rule broken" mode. If there IS a
					// callback then we want to gather ALL validation rule breaks and pass them to the caller
					if (_optionalBrokenFileCallback == null)
					{
						try
						{
							ruleForCompiledContent.EnsureRulesAreMet(compiledContentFragments);
						}
						catch (BrokenRuleEncounteredException e)
						{
							throw new BrokenRuleEncounteredInFileException(e, StyleSheetTypeOptions.Combined, relativePath);
						}
					}
					else
					{
						foreach (var brokenRule in ruleForCompiledContent.GetAnyBrokenRules(compiledContentFragments))
						{
							_optionalBrokenFileCallback(
								new BrokenRuleEncounteredInFileException(brokenRule, StyleSheetTypeOptions.Combined, relativePath)
							);
						}
					}
				}
			}

			// Similarly rules for Combined content are processed (the combined content is retrieved from the ContentRecordingTextFileLoader)
			var rulesForCombinedContent = _rules.Where(r => r.DoesThisRuleApplyTo(StyleSheetTypeOptions.Combined));
			if (rulesForCombinedContent.Any())
			{
				string combinedContent;
				lock (loadedContentBuilder)
				{
					combinedContent = loadedContentBuilder.ToString();
				}
				if (combinedContent != "")
				{
					var combinedContentFragments = LessCssHierarchicalParser.ParseIntoStructuredData(
						Parser.ParseLESS(combinedContent)
					);
					foreach (var ruleForCombinedContent in rulesForCombinedContent)
					{
 
						// If there's no callback then we're in the "throw as soon as validation rule broken" mode. If there IS a
						// callback then we want to gather ALL validation rule breaks and pass them to the caller
						if (_optionalBrokenFileCallback == null)
						{
							try
							{
								ruleForCombinedContent.EnsureRulesAreMet(combinedContentFragments);
							}
							catch (BrokenRuleEncounteredException e)
							{
								throw new BrokenRuleEncounteredInFileException(e, StyleSheetTypeOptions.Combined, relativePath);
							}
						}
						else
						{
							foreach (var brokenRule in ruleForCombinedContent.GetAnyBrokenRules(combinedContentFragments))
							{
								_optionalBrokenFileCallback(
									new BrokenRuleEncounteredInFileException(brokenRule, StyleSheetTypeOptions.Combined, relativePath)
								);
							}
						}
					}
				}
			}

			// Now that the rules validation process is complete, the compiled content can be returned
			return compiledContent;
		}

		/// <summary>
		/// This will wrap an ITextFileLoader and make a callback between loading the content and returning it from the Load method, if an
		/// exception is raised from this callback then it will be allowed to bubble up and content will not be returned
		/// </summary>
		private class CallbackMakingTextFileLoader : ITextFileLoader
		{
			private readonly ITextFileLoader _contentLoader;
			private readonly Action<TextFileContents> _callback;
			public CallbackMakingTextFileLoader(ITextFileLoader contentLoader, Action<TextFileContents> callback)
			{
				if (contentLoader == null)
					throw new ArgumentNullException("contentLoader");
				if (callback == null)
					throw new ArgumentNullException("callback");

				_contentLoader = contentLoader;
				_callback = callback;
			}

			public TextFileContents Load(string relativePath)
			{
				if (string.IsNullOrWhiteSpace(relativePath))
					throw new ArgumentException("Null/blank relativePath specified");

				var content = _contentLoader.Load(relativePath);
				_callback(content);
				return content;
			}
		}
	}
}
