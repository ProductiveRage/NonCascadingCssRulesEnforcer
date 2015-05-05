using CSSParser.ExtendedLESSParser.ContentSections;
using NonCascadingCSSRulesEnforcer.Rules;
using UnitTests.Shared;
using Xunit;
using System.Linq;
using Xunit.Extensions;
using System.Collections.Generic;

namespace UnitTests.Rules
{
	public class HtmlTagScopingMustBeAppliedToNonResetsOrThemesSheetsTests
    {
        [Fact]
		public void EmptyContentIsAcceptable()
		{
			var content = new ICSSFragment[0];
			(new HtmlTagScopingMustBeAppliedToNonResetsOrThemesSheets()).EnsureRulesAreMet(content);
		}

		[Fact]
		public void HtmlTagWithNoNestedContentIsAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New("html").ToContainerFragment();
			(new HtmlTagScopingMustBeAppliedToNonResetsOrThemesSheets()).EnsureRulesAreMet(new[] { content });
		}

		[Fact]
		public void HtmlTagWithOnlyANestedDivIsAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New(
				"html",
				CSSFragmentBuilderSelector.New("div")
			).ToContainerFragment();

            (new HtmlTagScopingMustBeAppliedToNonResetsOrThemesSheets()).EnsureRulesAreMet(new[] { content });
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
        public void HtmlTagWithStylePropertiesAfterLessTagIsNotAcceptable()
        {
            var content = CSSFragmentBuilderSelector.New(
                "html",
                CSSFragmentBuilderStyleProperty.New("@backgroundColor", "black"),
                CSSFragmentBuilderStyleProperty.New("color", "@backgroundColor")
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

        [Fact]
        public void EmptyContentReturnsNoBrokenRules()
        {
            var content = new ICSSFragment[0];

            Assert.Empty((new HtmlTagScopingMustBeAppliedToNonResetsOrThemesSheets()).GetAnyBrokenRules(content));
        }

        [Theory, MemberData("GetAnyBrokenRulesErrorCountContent")]
        public void GetAnyBrokenRulesErrorCount(int Id, ICSSFragment content, int expectedErrors)
        {
            Assert.Equal(
                expectedErrors,
                (new HtmlTagScopingMustBeAppliedToNonResetsOrThemesSheets()).GetAnyBrokenRules(new[] { content }).Count()
            );
        }

        public static IEnumerable<object[]> GetAnyBrokenRulesErrorCountContent
        {
            get
            {
                return new[]
                {
                    new object[] {1,CSSFragmentBuilderSelector.New("html").ToContainerFragment(),0 },
                    new object[] {2,CSSFragmentBuilderSelector.New("html", CSSFragmentBuilderSelector.New("div")).ToContainerFragment(),0},
                    new object[] {3,CSSFragmentBuilderSelector.New("html", CSSFragmentBuilderSelector.New("@media print", CSSFragmentBuilderStyleProperty.New("color", "black"))).ToContainerFragment(),1},
                    new object[] {4,CSSFragmentBuilderSelector.New("html", CSSFragmentBuilderStyleProperty.New("color", "black")).ToContainerFragment(),1},
                    new object[] {5,CSSFragmentBuilderSelector.New("html", CSSFragmentBuilderStyleProperty.New("@backgroundColor", "black"), CSSFragmentBuilderStyleProperty.New("color", "@backgroundColor")).ToContainerFragment(),1}
                };
            }
        }
    }
}
