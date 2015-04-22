using System;
using System.IO;
using CSSMinifier.FileLoaders;
using CSSMinifier.FileLoaders.Factories;
using CSSMinifier.Logging;
using CSSMinifier.PathMapping;
using NonCascadingCSSRulesEnforcer.CSSMinifierIntegration;
using NonCascadingCSSRulesEnforcer.Rules;
using CSSParser.ExtendedLESSParser;
using CSSParser;
using UnitTests.Rules;

namespace Tester
{
	class Program
	{
		static void Main(string[] args)
		{
			(new HtmlTagScopingMustBeAppliedToNonResetsOrThemesSheetsTests()).EmptyContentIsAcceptable();
			(new HtmlTagScopingMustBeAppliedToNonResetsOrThemesSheetsTests()).HtmlTagWithMediaQueryWrappedStylePropertiesIsNotAcceptable();
			(new HtmlTagScopingMustBeAppliedToNonResetsOrThemesSheetsTests()).HtmlTagWithNoNestedContentIsAcceptable();
			(new HtmlTagScopingMustBeAppliedToNonResetsOrThemesSheetsTests()).HtmlTagWithOnlyANestedDivIsAcceptable();
			(new HtmlTagScopingMustBeAppliedToNonResetsOrThemesSheetsTests()).HtmlTagWithStylePropertiesIsNotAcceptable();

			//(new NoSelectorMayBeRepeatedInTheRules(NoSelectorMayBeRepeatedInTheRules.ConformityOptions.AllowBareSelectorsToBeRepeated)).EnsureRulesAreMet(
			//	LessCssHierarchicalParser.ParseIntoStructuredData(
			//		Parser.ParseLESS("div.Header, div, div.Footer { div.What1, p, div.What2 { color: red; } } div.Header p { color: blue; }")
			//	)
			//);

			(new NoSelectorMayBeRepeatedInTheRulesTests()).EmptyContentIsFine();
			(new NoSelectorMayBeRepeatedInTheRulesTests()).MultipleSelectorsThatAreDifferentAreFine();
			(new NoSelectorMayBeRepeatedInTheRulesTests()).NullInputWillThrowAnException();
			(new NoSelectorMayBeRepeatedInTheRulesTests()).RepeatedSelectorAppearingInDifferentMediaQueriesIsFine();
			(new NoSelectorMayBeRepeatedInTheRulesTests()).RepeatedSelectorAppearingInDifferentNestedMediaQueriesIsFine();
			(new NoSelectorMayBeRepeatedInTheRulesTests()).RepeatedSelectorsWithDifferentStylesIsNotAcceptable();
			(new NoSelectorMayBeRepeatedInTheRulesTests()).SinglePropertyIsFine();


			(new NoSelectorMayBeRepeatedInTheRules(NoSelectorMayBeRepeatedInTheRules.ConformityOptions.AllowBareSelectorsToBeRepeated)).EnsureRulesAreMet(
				LessCssHierarchicalParser.ParseIntoStructuredData(
					Parser.ParseLESS("div.Header, div.Footer { div.What1, div.What2 { color: red; } }")
				)
			);


            var aux = (new HtmlTagScopingMustBeAppliedToNonResetsOrThemesSheets()).GetAnyBrokenRules(
                LessCssHierarchicalParser.ParseIntoStructuredData(
                    Parser.ParseLESS("div.Header, div.Footer { div.What1, div.What2 { color: red; } }")
                )
            );


			//var pathMapper = new FixedPathMapper(@"C:\Users\Me\Documents\Visual Studio 2010\Projects\Blog\Blog\Content");
            //var pathMapper = new FixedPathMapper(@"D:\CSS\noncascadingcssrulesenforcer\Tester\Content");
            //var pathMapper = new FixedPathMapper(@"W:\ETWP sites\liverpool\liverpool.etwp.dev.nm\styles");
            var pathMapper = new FixedPathMapper(@"C:\Users\dpons\Desktop\TestBed");
            
            var loader = new RuleEnforcingCssFileLoader(
                new IEnforceRules[]
				{
					new AllMeasurementsMustBePixels(
						AllMeasurementsMustBePixels.ConformityOptions.AllowOneHundredPercentOnAnyElementAndProperty |
						AllMeasurementsMustBePixels.ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes,
						new[] { "div", "td", "th", "li" }
					),
					new HtmlTagScopingMustBeAppliedToNonResetsOrThemesSheets(),
					new BorderAndPaddingMayNotBeCombinedWithWidth(
						BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.AllowVerticalBorderAndPadding
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
					new PaddingMustBeFullySpecifiedIfSpecifiedAtAll()
				},
                relativePath =>
                {
                    var lowerCasedTrimmedRelativePath = relativePath.Trim().ToLower();
                    if (lowerCasedTrimmedRelativePath.EndsWith("resets.css") || lowerCasedTrimmedRelativePath.EndsWith("resets.less"))
                        return StyleSheetTypeOptions.Reset;
                    else if (lowerCasedTrimmedRelativePath.EndsWith("theme.css") || lowerCasedTrimmedRelativePath.EndsWith("theme.less")
                    || lowerCasedTrimmedRelativePath.EndsWith("breakpoints.css") || lowerCasedTrimmedRelativePath.EndsWith("breakpoints.less")
                    || lowerCasedTrimmedRelativePath.EndsWith("mixinsandvalues.css") || lowerCasedTrimmedRelativePath.EndsWith("mixinsandvalues.less"))
                        return StyleSheetTypeOptions.Themes;
                    else
                        return StyleSheetTypeOptions.Other;
                },
                new SimpleTextFileContentLoader(pathMapper),
                baseContentLoader =>
                    new EnhancedNonCachedLessCssLoaderFactory(
                        baseContentLoader,
                        SourceMappingMarkerInjectionOptions.DoNotInject,
                        ErrorBehaviourOptions.LogAndRaiseException,
                        new NullLogger()
                    ).Get(),
                    PrintError
            );
            Console.WriteLine(
                loader.Load("Styles.less").Content
            );
			Console.ReadLine();
		}

        static void PrintError(BrokenRuleEncounteredInFileException param)
        {
            Console.WriteLine(param.Message);
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

		/*
		private class FixedContentTextFileLoader : ITextFileLoader
		{
			private TextFileContents[] _files;
			public FixedContentTextFileLoader(IEnumerable<TextFileContents> files)
			{
				if (files == null)
					throw new ArgumentNullException("files");

				_files = files.ToArray();
				if (_files.Any(f => f == null))
					throw new ArgumentException("Null reference encountered in files set");
			}

			/// <summary>
			/// This will never return null, it will throw an exception for a null or empty relativePath - it is up to the particular implementation whether or not to throw
			/// an exception for invalid / inaccessible filenames (if no exception is thrown, the issue should be logged). It is up the the implementation to handle mapping
			/// the relative path to a full file path.
			/// </summary>
			public TextFileContents Load(string relativePath)
			{
				if (string.IsNullOrWhiteSpace(relativePath))
					throw new ArgumentException("Null/blank relativePath specified");

				var file = _files.FirstOrDefault(f => f.RelativePath == relativePath);
				if (file == null)
					throw new ArgumentException("No content available for " + relativePath);
				return file;
			}
		}
		 */
	}
}
