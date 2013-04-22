using CSSParser.ExtendedLESSParser;
using NonCascadingCSSRulesEnforcer.Rules;
using UnitTests.Shared;
using Xunit;

namespace UnitTests.Rules
{
	public class NoBareSelectorsInNonResetsOrThemeSheetsTests
	{
		[Fact]
		public void EmptyContentIsAcceptable_ScopeRestrictingBodyTagAllowed()
		{
			var content = new ICSSFragment[0];

			Assert.DoesNotThrow(() =>
			{
				(new NoBareSelectorsInNonResetsOrThemeSheets(NoBareSelectorsInNonResetsOrThemeSheets.ScopeRestrictingBodyTagBehaviourOptions.Allow)).EnsureRulesAreMet(content);
			});
		}

		[Fact]
		public void EmptyContentIsAcceptable_ScopeRestrictingBodyTagDisallowed()
		{
			var content = new ICSSFragment[0];

			Assert.DoesNotThrow(() =>
			{
				(new NoBareSelectorsInNonResetsOrThemeSheets(NoBareSelectorsInNonResetsOrThemeSheets.ScopeRestrictingBodyTagBehaviourOptions.Disallow)).EnsureRulesAreMet(content);
			});
		}

		[Fact]
		public void BodyTagWithNoNestedContentIsAcceptable_ScopeRestrictingBodyTagAllowed()
		{
			var content = CSSFragmentBuilderSelector.New("body").ToContainerFragment();

			Assert.DoesNotThrow(() =>
			{
				(new NoBareSelectorsInNonResetsOrThemeSheets(NoBareSelectorsInNonResetsOrThemeSheets.ScopeRestrictingBodyTagBehaviourOptions.Allow)).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void BodyTagWithNoNestedContentIsNotAcceptable_ScopeRestrictingBodyTagDisallowed()
		{
			var content = CSSFragmentBuilderSelector.New("body").ToContainerFragment();

			Assert.Throws<NoBareSelectorsInNonResetsOrThemeSheets.DisallowBareSelectorsEncounteredException>(() =>
			{
				(new NoBareSelectorsInNonResetsOrThemeSheets(NoBareSelectorsInNonResetsOrThemeSheets.ScopeRestrictingBodyTagBehaviourOptions.Disallow)).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void BodyTagWithStylePropertoesIsNotAcceptable_ScopeRestrictingBodyTagAllowed()
		{
			var content = CSSFragmentBuilderSelector.New(
				"body",
				CSSFragmentBuilderStyleProperty.New("color", "black")
			).ToContainerFragment();

			Assert.Throws<NoBareSelectorsInNonResetsOrThemeSheets.DisallowBareSelectorsEncounteredException>(() =>
			{
				(new NoBareSelectorsInNonResetsOrThemeSheets(NoBareSelectorsInNonResetsOrThemeSheets.ScopeRestrictingBodyTagBehaviourOptions.Allow)).EnsureRulesAreMet(new[] { content });
			});
		}
	}
}
