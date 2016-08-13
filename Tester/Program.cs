using System;
using System.IO;
using CSSMinifier.FileLoaders;
using CSSMinifier.FileLoaders.Factories;
using CSSMinifier.Logging;
using CSSMinifier.PathMapping;
using NonCascadingCSSRulesEnforcer.CSSMinifierIntegration;
using NonCascadingCSSRulesEnforcer.Rules;
using NonCascadingCSSRulesEnforcer.Rules.Compatibility;

namespace Tester
{
	class Program
	{
		static void Main(string[] args)
		{
			// If this was validating style sheets on-the-fly in a web application then an IRelativePathMapper implementation should be used that uses Server.MapPath (or
			// equivalent) but for the code here, we'll just be reading from the "BlogStyles" folder (which has the style sheets from my Blog - which seems like a nice
			// compromise; non-trivial but not too large to mentally easily absorb in its entirety)
			var pathMapper = new FixedPathMapper(basePath: new DirectoryInfo("BlogStyles").FullName);

			// This is the full set of validation rules, as described at http://www.productiverage.com/noncascading-css-a-revolution (with a bonus rule added that ensures
			// that enormous style sheets don't exceed the selector limit for old versions of IE - if that limit is exceeded then the later rules are silently ignored,
			// which can go unnoticed for some time and be very annoying once discovered!)
			var fullSetOfValidationRules = new IEnforceRules[]
			{
				new AllMeasurementsMustBePixels(
					AllMeasurementsMustBePixels.ConformityOptions.AllowOneHundredPercentOnAnyElementAndProperty |
					AllMeasurementsMustBePixels.ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes,
					new[] { "div", "td", "th", "li" }
				),
				new HtmlTagScopingMustBeAppliedToNonResetsOrThemesSheets(),
				new BorderAndPaddingMayNotBeCombinedWithWidth(
					BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.AllowVerticalBorderAndPadding |
					BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.IgnoreRuleIfBorderBoxSizingRulePresent
				),
				new MarginMustBeFullySpecifiedIfSpecifiedAtAll(),
				new NoBareSelectorsInNonResetsOrThemeSheets(
					NoBareSelectorsInNonResetsOrThemeSheets.ScopeRestrictingHtmlTagBehaviourOptions.Allow
				),
				new NoMediaQueriesInResetsAndThemeSheets(),
				new NoSelectorMayBeRepeatedInTheRules(
					NoSelectorMayBeRepeatedInTheRules.ConformityOptions.AllowBareSelectorsToBeRepeated
				),
				new OnlyBareSelectorsInResetsAndThemeSheets(
					OnlyBareSelectorsInResetsAndThemeSheets.ConformityOptions.AllowLessCssMixins
				),
				new PaddingMustBeFullySpecifiedIfSpecifiedAtAll(),
				new LegacyIESelectorLimitMustBeRespected()
			};

			// Declare the stylesheet loaded - since this uses the EnhancedNonCachedLessCssLoaderFactory, the @imports will all be flattened and the LESS content processed
			// into regular CSS. Any validation rules that are broken will be recorded as the process takes place. 
			var wereAnyRulesBroken = false;
			var loader = new RuleEnforcingCssFileLoader(
				rules: fullSetOfValidationRules,
				styleSheetTypeDeterminer: StyleSheetCategoriser,
				baseContentLoader: new SimpleTextFileContentLoader(pathMapper),
				compilerGenerator: baseContentLoader =>
					new EnhancedNonCachedLessCssLoaderFactory(
						baseContentLoader,
						SourceMappingMarkerInjectionOptions.Inject, // This includes extra selectors into the final rules (eg. "#Header.less_37"), a bit like a poor man's source map
						ErrorBehaviourOptions.LogAndRaiseException,
						new NullLogger()
					)
					.Get(),
				optionalBrokenFileCallback: brokenRule =>
				{
					Console.WriteLine(brokenRule.Message);
					wereAnyRulesBroken = true;
				}
			);

			// Load and process the example style sheet - if no rules are broken then show the final generated (ie. @import-flattened, LESS-processed, minified) content. If
			// any validation rules WERE broken then print them out instead.
			var content = loader.Load("Styles.css");
			if (!wereAnyRulesBroken)
			{
				Console.WriteLine("No rules were broken - hurrah! Press [Enter] to show final content..");
				Console.ReadLine();
				Console.WriteLine();
				Console.WriteLine(content.Content);
				Console.WriteLine();
				Console.WriteLine("Press [Enter] to terminate..");
				Console.ReadLine();
				return;
			}
			Console.WriteLine();
			Console.WriteLine("Validation complete, press [Enter] to terminate..");
			Console.ReadLine();
		}

		/// <summary>
		/// Different types of style sheet require different validation (eg. Reset sheets should only contain bare selectors while most other style sheets should NOT contain
		/// any bare selectors). The style sheet type is determined by its filename, based upon some conventions which seems to work after applying these rules to various sites.
		/// Alternative naming schemes could be used, the RuleEnforcingCssFileLoader just needs a delegate that can tell it what style sheet type any given filename should be
		/// treated as.
		/// </summary>
		private static StyleSheetTypeOptions StyleSheetCategoriser(string relativePath)
		{
			if (relativePath == null)
				throw new ArgumentNullException(nameof(relativePath));

			if (relativePath.EndsWith("resets.css", StringComparison.OrdinalIgnoreCase)
			|| relativePath.EndsWith("resets.less", StringComparison.OrdinalIgnoreCase))
				return StyleSheetTypeOptions.Reset;

			if (relativePath.EndsWith("theme.css", StringComparison.OrdinalIgnoreCase)
			|| relativePath.EndsWith("theme.less", StringComparison.OrdinalIgnoreCase)
			|| relativePath.EndsWith("breakpoints.css", StringComparison.OrdinalIgnoreCase)
			|| relativePath.EndsWith("breakpoints.less", StringComparison.OrdinalIgnoreCase)
			|| relativePath.EndsWith("mixinsandvalues.css", StringComparison.OrdinalIgnoreCase)
			|| relativePath.EndsWith("mixinsandvalues.less", StringComparison.OrdinalIgnoreCase))
				return StyleSheetTypeOptions.Themes;

			return StyleSheetTypeOptions.Other;
		}

		private class FixedPathMapper : IRelativePathMapper
		{
			private readonly string _basePath;
			public FixedPathMapper(string basePath)
			{
				if (string.IsNullOrWhiteSpace(basePath))
					throw new ArgumentException("Null/blank basePath specified");

				_basePath = basePath.Trim();
			}

			public string MapPath(string relativePath)
			{
				if (string.IsNullOrWhiteSpace(relativePath))
					throw new ArgumentException("Null/blank relativePath specified");

				return Path.Combine(_basePath, relativePath);
			}
		}
	}
}
