using CSSParser.ExtendedLESSParser;
using NonCascadingCSSRulesEnforcer.Rules;
using UnitTests.Shared;
using Xunit;

namespace UnitTests.Rules
{
	public class NoBareSelectorsInNonResetsOrThemeSheetsTests
	{
		[Fact]
		public void EmptyContentIsAcceptable_ScopeRestrictingHtmlTagAllowed()
		{
			var content = new ICSSFragment[0];

			Assert.DoesNotThrow(() =>
			{
				(new NoBareSelectorsInNonResetsOrThemeSheets(NoBareSelectorsInNonResetsOrThemeSheets.ScopeRestrictingHtmlTagBehaviourOptions.Allow)).EnsureRulesAreMet(content);
			});
		}

		[Fact]
		public void EmptyContentIsAcceptable_ScopeRestrictingHtmlTagDisallowed()
		{
			var content = new ICSSFragment[0];

			Assert.DoesNotThrow(() =>
			{
				(new NoBareSelectorsInNonResetsOrThemeSheets(NoBareSelectorsInNonResetsOrThemeSheets.ScopeRestrictingHtmlTagBehaviourOptions.Disallow)).EnsureRulesAreMet(content);
			});
		}

		[Fact]
		public void HtmlTagWithNoNestedContentIsAcceptable_ScopeRestrictingHtmlTagAllowed()
		{
			var content = CSSFragmentBuilderSelector.New("html").ToContainerFragment();

			Assert.DoesNotThrow(() =>
			{
				(new NoBareSelectorsInNonResetsOrThemeSheets(NoBareSelectorsInNonResetsOrThemeSheets.ScopeRestrictingHtmlTagBehaviourOptions.Allow)).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void HtmlTagWithNoNestedContentIsNotAcceptable_ScopeRestrictingHtmlTagDisallowed()
		{
			var content = CSSFragmentBuilderSelector.New("html").ToContainerFragment();

			Assert.Throws<NoBareSelectorsInNonResetsOrThemeSheets.DisallowBareSelectorsEncounteredException>(() =>
			{
				(new NoBareSelectorsInNonResetsOrThemeSheets(NoBareSelectorsInNonResetsOrThemeSheets.ScopeRestrictingHtmlTagBehaviourOptions.Disallow)).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void HtmlTagWithStylePropertoesIsNotAcceptable_ScopeRestrictingHtmlTagAllowed()
		{
			var content = CSSFragmentBuilderSelector.New(
				"html",
				CSSFragmentBuilderStyleProperty.New("color", "black")
			).ToContainerFragment();

			Assert.Throws<NoBareSelectorsInNonResetsOrThemeSheets.DisallowBareSelectorsEncounteredException>(() =>
			{
				(new NoBareSelectorsInNonResetsOrThemeSheets(NoBareSelectorsInNonResetsOrThemeSheets.ScopeRestrictingHtmlTagBehaviourOptions.Allow)).EnsureRulesAreMet(new[] { content });
			});
		}
	}
}
