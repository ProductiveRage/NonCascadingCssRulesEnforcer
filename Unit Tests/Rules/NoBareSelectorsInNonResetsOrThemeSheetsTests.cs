using System.Collections.Generic;
using System.Linq;
using CSSParser.ExtendedLESSParser.ContentSections;
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

			(new NoBareSelectorsInNonResetsOrThemeSheets(NoBareSelectorsInNonResetsOrThemeSheets.ScopeRestrictingHtmlTagBehaviourOptions.Allow)).EnsureRulesAreMet(content);
		}

		[Fact]
		public void EmptyContentIsAcceptable_ScopeRestrictingHtmlTagDisallowed()
		{
			var content = new ICSSFragment[0];

			(new NoBareSelectorsInNonResetsOrThemeSheets(NoBareSelectorsInNonResetsOrThemeSheets.ScopeRestrictingHtmlTagBehaviourOptions.Disallow)).EnsureRulesAreMet(content);
		}

		[Fact]
		public void HtmlTagWithNoNestedContentIsAcceptable_ScopeRestrictingHtmlTagAllowed()
		{
			var content = CSSFragmentBuilderSelector.New("html").ToContainerFragment();

			(new NoBareSelectorsInNonResetsOrThemeSheets(NoBareSelectorsInNonResetsOrThemeSheets.ScopeRestrictingHtmlTagBehaviourOptions.Allow)).EnsureRulesAreMet(new[] { content });
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

		[Theory, MemberData("GetAnyBrokenRulesAllowContent")]
		public void GetAnyBrokenRulesAllowCount(int Id, ICSSFragment content, int expectedErrors)
		{
			Assert.Equal(
				expectedErrors,
				PaddingMustBeFullySpecifiedIfSpecifiedAtAll.Instance.GetAnyBrokenRules(new[] { content }).Count()
			);
		}

		public static IEnumerable<object[]> GetAnyBrokenRulesAllowContent
		{
			get
			{
				return new[]
				{
					new object[] {1,CSSFragmentBuilderSelector.New("html").ToContainerFragment(),0},
					new object[] {2,CSSFragmentBuilderSelector.New("html", CSSFragmentBuilderStyleProperty.New("color", "black")).ToContainerFragment(),0}
				};
			}
		}

		[Theory, MemberData("GetAnyBrokenRulesDisallowContent")]
		public void GetAnyBrokenRulesDisallowCount(int Id, ICSSFragment content, int expectedErrors)
		{
			Assert.Equal(
				expectedErrors,
				PaddingMustBeFullySpecifiedIfSpecifiedAtAll.Instance.GetAnyBrokenRules(new[] { content }).Count()
			);
		}

		public static IEnumerable<object[]> GetAnyBrokenRulesDisallowContent
		{
			get
			{
				return new[]
				{
					new object[] {1,CSSFragmentBuilderSelector.New("html").ToContainerFragment(),0}
				};
			}
		}
	}
}
