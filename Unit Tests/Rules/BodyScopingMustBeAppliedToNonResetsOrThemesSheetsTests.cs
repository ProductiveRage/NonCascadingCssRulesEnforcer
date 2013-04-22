using CSSParser.ExtendedLESSParser;
using NonCascadingCSSRulesEnforcer.Rules;
using UnitTests.Shared;
using Xunit;

namespace UnitTests.Rules
{
	public class BodyScopingMustBeAppliedToNonResetsOrThemesSheetsTests
	{
		[Fact]
		public void EmptyContentIsAcceptable()
		{
			var content = new ICSSFragment[0];

			Assert.DoesNotThrow(() =>
			{
				(new BodyScopingMustBeAppliedToNonResetsOrThemesSheets()).EnsureRulesAreMet(content);
			});
		}

		[Fact]
		public void BodyTagWithNoNestedContentIsAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New("body").ToContainerFragment();

			Assert.DoesNotThrow(() =>
			{
				(new BodyScopingMustBeAppliedToNonResetsOrThemesSheets()).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void BodyTagWithOnlyANestedDivIsAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New(
				"body",
				CSSFragmentBuilderSelector.New("div")
			).ToContainerFragment();

			Assert.DoesNotThrow(() =>
			{
				(new BodyScopingMustBeAppliedToNonResetsOrThemesSheets()).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void BodyTagWithStylePropertiesIsNotAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New(
				"body",
				CSSFragmentBuilderStyleProperty.New("color", "black")
			).ToContainerFragment();

			Assert.Throws<BodyScopingMustBeAppliedToNonResetsOrThemesSheets.ScopeRestrictingBodyTagNotAppliedException>(() =>
			{
				(new BodyScopingMustBeAppliedToNonResetsOrThemesSheets()).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void BodyTagWithMediaQueryWrappedStylePropertiesIsNotAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New(
				"body",
				CSSFragmentBuilderSelector.New(
					"@media print",
					CSSFragmentBuilderStyleProperty.New("color", "black")
				)
			).ToContainerFragment();

			Assert.Throws<BodyScopingMustBeAppliedToNonResetsOrThemesSheets.ScopeRestrictingBodyTagNotAppliedException>(() =>
			{
				(new BodyScopingMustBeAppliedToNonResetsOrThemesSheets()).EnsureRulesAreMet(new[] { content });
			});
		}
	}
}
