using CSSParser.ExtendedLESSParser.ContentSections;
using NonCascadingCSSRulesEnforcer.Rules;
using System.Collections.Generic;
using UnitTests.Shared;
using Xunit;
using System.Linq;

namespace UnitTests.Rules
{
    public class BorderAndPaddingMayNotBeCombinedWithWidthTests
    {
        [Fact]
        public void PaddingOnlyIsFine()
        {
            var content = CSSFragmentBuilderSelector.New(
                "div",
                CSSFragmentBuilderStyleProperty.New("padding", "16px")
            ).ToContainerFragment();

            (new BorderAndPaddingMayNotBeCombinedWithWidth(BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict)).EnsureRulesAreMet(new[] { content });
        }

        [Fact]
        public void PaddingWithWidthAutoIsFine()
        {
            var content = CSSFragmentBuilderSelector.New(
                "div",
                CSSFragmentBuilderStyleProperty.New("padding", "16px"),
                CSSFragmentBuilderStyleProperty.New("width", "auto")
            ).ToContainerFragment();

            (new BorderAndPaddingMayNotBeCombinedWithWidth(BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict)).EnsureRulesAreMet(new[] { content });
        }

        [Fact]
        public void PaddingWithWidthAutoSpecifiedLastIsFine()
        {
            var content = CSSFragmentBuilderSelector.New(
                "div",
                CSSFragmentBuilderStyleProperty.New("padding", "16px"),
                CSSFragmentBuilderStyleProperty.New("width", "200px"),
                CSSFragmentBuilderStyleProperty.New("width", "auto")
            ).ToContainerFragment();

            (new BorderAndPaddingMayNotBeCombinedWithWidth(BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict)).EnsureRulesAreMet(new[] { content });
        }

        [Fact]
        public void PaddingWithWidthAutoSpecifiedAsImportantIsFine()
        {
            var content = CSSFragmentBuilderSelector.New(
                "div",
                CSSFragmentBuilderStyleProperty.New("padding", "16px"),
                CSSFragmentBuilderStyleProperty.New("width", "auto !important"),
                CSSFragmentBuilderStyleProperty.New("width", "200px")
            ).ToContainerFragment();

            (new BorderAndPaddingMayNotBeCombinedWithWidth(BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict)).EnsureRulesAreMet(new[] { content });
        }

        [Fact]
        public void WidthOnlyIsFine()
        {
            var content = CSSFragmentBuilderSelector.New(
                "div",
                CSSFragmentBuilderStyleProperty.New("width", "320px")
            ).ToContainerFragment();

            (new BorderAndPaddingMayNotBeCombinedWithWidth(BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict)).EnsureRulesAreMet(new[] { content });
        }

        [Fact]
        public void WidthWithZeroPaddingIsFine()
        {
            var content = CSSFragmentBuilderSelector.New(
                "div",
                CSSFragmentBuilderStyleProperty.New("width", "320px"),
                CSSFragmentBuilderStyleProperty.New("padding", "0")
            ).ToContainerFragment();

            (new BorderAndPaddingMayNotBeCombinedWithWidth(BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict)).EnsureRulesAreMet(new[] { content });
        }

        [Fact]
        public void WidthWithZeroPixelPaddingIsFine()
        {
            var content = CSSFragmentBuilderSelector.New(
                "div",
                CSSFragmentBuilderStyleProperty.New("width", "320px"),
                CSSFragmentBuilderStyleProperty.New("padding", "0px")
            ).ToContainerFragment();

            (new BorderAndPaddingMayNotBeCombinedWithWidth(BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict)).EnsureRulesAreMet(new[] { content });
        }

        [Fact]
        public void WidthWithNonZeroPixelPaddingIsNotAllowed()
        {
            var content = CSSFragmentBuilderSelector.New(
                "div",
                CSSFragmentBuilderStyleProperty.New("width", "320px"),
                CSSFragmentBuilderStyleProperty.New("padding", "16px")
            ).ToContainerFragment();

            Assert.Throws<BorderAndPaddingMayNotBeCombinedWithWidth.BorderAndPaddingMayNotBeCombinedWithWidthException>(() =>
            {
                (new BorderAndPaddingMayNotBeCombinedWithWidth(BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict)).EnsureRulesAreMet(new[] { content });
            });
        }

        [Fact]
        public void WidthWithNonZeroPixelPaddingIsAllowedIfBorderBoxIsEnabled()
        {
            var content = CSSFragmentBuilderSelector.New(
                "div",
                CSSFragmentBuilderStyleProperty.New("box-sizing", "border-box"),
                CSSFragmentBuilderStyleProperty.New("width", "320px"),
                CSSFragmentBuilderStyleProperty.New("padding", "16px")
            ).ToContainerFragment();

            var confirmity = BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.IgnoreRuleIfBorderBoxSizingRulePresent;
            (new BorderAndPaddingMayNotBeCombinedWithWidth(confirmity)).EnsureRulesAreMet(new[] { content });
        }

        [Fact]
        public void WidthWithBorderNoneIsFine()
        {
            var content = CSSFragmentBuilderSelector.New(
                "div",
                CSSFragmentBuilderStyleProperty.New("width", "320px"),
                CSSFragmentBuilderStyleProperty.New("border", "none")
            ).ToContainerFragment();

                (new BorderAndPaddingMayNotBeCombinedWithWidth(BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict)).EnsureRulesAreMet(new[] { content });
        }

        [Fact]
        public void WidthWithZeroWidthBorder()
        {
            var content = CSSFragmentBuilderSelector.New(
                "div",
                CSSFragmentBuilderStyleProperty.New("width", "320px"),
                CSSFragmentBuilderStyleProperty.New("border", "0 solid black")
            ).ToContainerFragment();

            (new BorderAndPaddingMayNotBeCombinedWithWidth(BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict)).EnsureRulesAreMet(new[] { content });
        }

        [Fact]
        public void WidthWithZeroPixelWidthBorder()
        {
            var content = CSSFragmentBuilderSelector.New(
                "div",
                CSSFragmentBuilderStyleProperty.New("width", "320px"),
                CSSFragmentBuilderStyleProperty.New("border", "0px solid black")
            ).ToContainerFragment();

            (new BorderAndPaddingMayNotBeCombinedWithWidth(BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict)).EnsureRulesAreMet(new[] { content });
        }

        /// <summary>
        /// Setting a border-radius property should not be marked as invalid since it doesn't affect the dimensions of the element (and may
        /// still affect the rendering even if there is zero border width)
        /// </summary>
        [Fact]
        public void WidthWithBorderRadiusIsFine()
        {
            var content = CSSFragmentBuilderSelector.New(
                "div",
                CSSFragmentBuilderStyleProperty.New("width", "320px"),
                CSSFragmentBuilderStyleProperty.New("border-radius", "8px")
            ).ToContainerFragment();

            (new BorderAndPaddingMayNotBeCombinedWithWidth(BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict)).EnsureRulesAreMet(new[] { content });
        }

        [Fact]
        public void VertialPaddingWithWidthIsNotAllowedIfAllowVerticalBorderAndPaddingNotIsEnabled()
        {
            var content = CSSFragmentBuilderSelector.New(
                "div",
                CSSFragmentBuilderStyleProperty.New("width", "100%"),
                CSSFragmentBuilderStyleProperty.New("padding", "2px 0")
            ).ToContainerFragment();

            Assert.Throws<BorderAndPaddingMayNotBeCombinedWithWidth.BorderAndPaddingMayNotBeCombinedWithWidthException>(() =>
            {
                (new BorderAndPaddingMayNotBeCombinedWithWidth(
                    BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict
                )).EnsureRulesAreMet(new[] { content });
            });
        }

        [Fact]
        public void VertialPaddingWithWidthIsFineIfAllowVerticalBorderAndPaddingIsEnabled()
        {
            var content = CSSFragmentBuilderSelector.New(
                "div",
                CSSFragmentBuilderStyleProperty.New("padding", "16px 0"),
                CSSFragmentBuilderStyleProperty.New("width", "100%")
            ).ToContainerFragment();

            (new BorderAndPaddingMayNotBeCombinedWithWidth(
                BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.AllowVerticalBorderAndPadding
            )).EnsureRulesAreMet(new[] { content });
        }

        [Theory, MemberData("GetAnyBrokenRulesAllowVerticalContent")]
        public void GetAnyBrokenRulesAllowVertical(int Id, ICSSFragment content, int expectedErrors)
        {
            Assert.Equal(
                expectedErrors,
                (new BorderAndPaddingMayNotBeCombinedWithWidth(
                BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.AllowVerticalBorderAndPadding
                    )).GetAnyBrokenRules(new[] { content }).Count()
                );
        }

        public static IEnumerable<object[]> GetAnyBrokenRulesAllowVerticalContent
        {
            get
            {
                return new[]
                {
                new object[] {13,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("width", "320px"),CSSFragmentBuilderStyleProperty.New("padding", "16px")).ToContainerFragment(),1},
                new object[] {19,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("padding", "16px 0"),CSSFragmentBuilderStyleProperty.New("width", "100%")).ToContainerFragment(),0}
                
                };
            }
        }

        [Theory, MemberData("GetAnyBrokenRulesStrictContent")]
        public void GetAnyBrokenRulesStrictErrorCount(int Id, ICSSFragment content, int expectedErrors)
        {
            Assert.Equal(
                expectedErrors,
                (new BorderAndPaddingMayNotBeCombinedWithWidth(
                BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict
                    )).GetAnyBrokenRules(new[] { content }).Count()
                );
        }

        public static IEnumerable<object[]> GetAnyBrokenRulesStrictContent
        {
            get
            {
                return new[]
                {
                new object[] {1,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("padding", "16px")).ToContainerFragment(),0 },
                new object[] {2,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("padding", "16px"),CSSFragmentBuilderStyleProperty.New("width", "auto")).ToContainerFragment(),0},
                new object[] {3,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("padding", "16px"),CSSFragmentBuilderStyleProperty.New("width", "200px"),CSSFragmentBuilderStyleProperty.New("width", "auto")).ToContainerFragment(),0},
                new object[] {4,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("padding", "16px"),CSSFragmentBuilderStyleProperty.New("width", "auto !important"),CSSFragmentBuilderStyleProperty.New("width", "200px")).ToContainerFragment(),0},
                new object[] {5,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("width", "320px")).ToContainerFragment(),0},
                new object[] {6,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("width", "320px"),CSSFragmentBuilderStyleProperty.New("padding", "0")).ToContainerFragment(),0},
                new object[] {7,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("width", "320px"),CSSFragmentBuilderStyleProperty.New("padding", "0px")).ToContainerFragment() ,0},
                new object[] {9,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("width", "320px"),CSSFragmentBuilderStyleProperty.New("padding", "16px")).ToContainerFragment() ,1},
                new object[] {13,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("width", "320px"),CSSFragmentBuilderStyleProperty.New("padding", "16px")).ToContainerFragment(),1},
                new object[] {14,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("width", "320px"),CSSFragmentBuilderStyleProperty.New("border", "none")).ToContainerFragment(),0},
                new object[] {15,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("width", "320px"),CSSFragmentBuilderStyleProperty.New("border", "0 solid black")).ToContainerFragment(),0},
                new object[] {16,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("width", "320px"),CSSFragmentBuilderStyleProperty.New("border", "0px solid black")).ToContainerFragment(),0},
                new object[] {17,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("width", "320px"),CSSFragmentBuilderStyleProperty.New("border-radius", "8px")).ToContainerFragment(),0},
                new object[] {18,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("width", "100%"),CSSFragmentBuilderStyleProperty.New("padding", "2px 0")).ToContainerFragment(),1},
                
                };
            }
        }

        [Theory, MemberData("GetAnyBrokenRulesBoxingErrorContent")]
        public void GetAnyBrokenRulesBorderBoxingErrorCount(int Id, ICSSFragment content, int expectedErrors)
        {
            Assert.Equal(
                expectedErrors,
                (new BorderAndPaddingMayNotBeCombinedWithWidth(
                BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.IgnoreRuleIfBorderBoxSizingRulePresent
                    )).GetAnyBrokenRules(new[] { content }).Count()
                );
        }

        public static IEnumerable<object[]> GetAnyBrokenRulesBoxingErrorContent
        {
            get
            {
                return new[]
                {
                new object[] {12,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("box-sizing", "border-box"),CSSFragmentBuilderStyleProperty.New("width", "320px"),CSSFragmentBuilderStyleProperty.New("padding", "16px")).ToContainerFragment(),0},
                new object[] {13,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("width", "320px"),CSSFragmentBuilderStyleProperty.New("padding", "16px")).ToContainerFragment(),1},
                new object[] {14,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("width", "320px"),CSSFragmentBuilderStyleProperty.New("border", "none")).ToContainerFragment(),0},
                new object[] {15,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("width", "320px"),CSSFragmentBuilderStyleProperty.New("border", "0 solid black")).ToContainerFragment(),0},
                new object[] {16,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("width", "320px"),CSSFragmentBuilderStyleProperty.New("border", "0px solid black")).ToContainerFragment(),0},
                new object[] {17,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("width", "320px"),CSSFragmentBuilderStyleProperty.New("border-radius", "8px")).ToContainerFragment(),0},
                new object[] {19,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("padding", "16px 0"),CSSFragmentBuilderStyleProperty.New("width", "100%")).ToContainerFragment(),0}
                
                };
            }
        }
    }
}
