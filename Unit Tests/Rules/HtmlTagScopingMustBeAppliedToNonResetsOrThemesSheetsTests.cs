using CSSParser.ExtendedLESSParser;
using NonCascadingCSSRulesEnforcer.Rules;
using UnitTests.Shared;
using Xunit;

namespace UnitTests.Rules
{
	public class HtmlTagScopingMustBeAppliedToNonResetsOrThemesSheetsTests
	{
		[Fact]
		public void EmptyContentIsAcceptable()
		{
			var content = new ICSSFragment[0];

			Assert.DoesNotThrow(() =>
			{
				(new HtmlTagScopingMustBeAppliedToNonResetsOrThemesSheets()).EnsureRulesAreMet(content);
			});
		}

		[Fact]
		public void HtmlTagWithNoNestedContentIsAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New("html").ToContainerFragment();

			Assert.DoesNotThrow(() =>
			{
				(new HtmlTagScopingMustBeAppliedToNonResetsOrThemesSheets()).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void HtmlTagWithOnlyANestedDivIsAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New(
				"html",
				CSSFragmentBuilderSelector.New("div")
			).ToContainerFragment();

			Assert.DoesNotThrow(() =>
			{
				(new HtmlTagScopingMustBeAppliedToNonResetsOrThemesSheets()).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void HtmlTagWithStylePropertiesIsNotAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New(
				"html",
				CSSFragmentBuilderStyleProperty.New("color", "black")
			).ToContainerFragment();

			Assert.Throws<HtmlTagScopingMustBeAppliedToNonResetsOrThemesSheets.ScopeRestrictingHtmlTagNotAppliedException>(() =>
			{
				(new HtmlTagScopingMustBeAppliedToNonResetsOrThemesSheets()).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void HtmlTagWithMediaQueryWrappedStylePropertiesIsNotAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New(
				"html",
				CSSFragmentBuilderSelector.New(
					"@media print",
					CSSFragmentBuilderStyleProperty.New("color", "black")
				)
			).ToContainerFragment();

			Assert.Throws<HtmlTagScopingMustBeAppliedToNonResetsOrThemesSheets.ScopeRestrictingHtmlTagNotAppliedException>(() =>
			{
				(new HtmlTagScopingMustBeAppliedToNonResetsOrThemesSheets()).EnsureRulesAreMet(new[] { content });
			});
		}
	}
}
