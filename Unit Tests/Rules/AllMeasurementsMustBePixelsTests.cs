﻿using System.Collections.Generic;
using System.Linq;
using CSSParser.ExtendedLESSParser.ContentSections;
using NonCascadingCSSRulesEnforcer.Rules;
using UnitTests.Shared;
using Xunit;

namespace UnitTests.Rules
{
	public class AllMeasurementsMustBePixelsTests
	{
		[Fact]
		public void PercentageWidthDivsMayBeAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("width", "50%")
			).ToContainerFragment();

			(new AllMeasurementsMustBePixels(
				AllMeasurementsMustBePixels.ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes,
				AllMeasurementsMustBePixels.RecommendedPercentageWidthExceptions
			)).EnsureRulesAreMet(new[] { content });
		}

		[Fact]
		public void HundredPercentageWidthImgsMayBeAcceptableIfNestedInPercentageWidthDivs()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("width", "50%"),
				CSSFragmentBuilderSelector.New(
					"img",
					CSSFragmentBuilderStyleProperty.New("width", "100%")
				)
			).ToContainerFragment();

				(new AllMeasurementsMustBePixels(
					AllMeasurementsMustBePixels.ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes,
					AllMeasurementsMustBePixels.RecommendedPercentageWidthExceptions
				)).EnsureRulesAreMet(new[] { content });
		}

		[Fact]
		public void HundredPercentageWidthImgsMayBeAcceptableIfDeeplyNestedInPercentageWidthDivs()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("width", "50%"),
				CSSFragmentBuilderSelector.New(
					"p",
					CSSFragmentBuilderSelector.New(
						"img",
						CSSFragmentBuilderStyleProperty.New("width", "100%")
					)
				)
			).ToContainerFragment();

				(new AllMeasurementsMustBePixels(
					AllMeasurementsMustBePixels.ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes,
					AllMeasurementsMustBePixels.RecommendedPercentageWidthExceptions
				)).EnsureRulesAreMet(new[] { content });
		}

		[Fact]
		public void HundredPercentageWidthImgsMayBeAcceptableIfPercentageWidthAppliedToDivsInMediaQuery()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("width", "500px"),
				CSSFragmentBuilderSelector.New(
					"@media screen and (max-width:70em)",
					CSSFragmentBuilderStyleProperty.New("width", "50%"),
					CSSFragmentBuilderSelector.New(
						"img",
						CSSFragmentBuilderStyleProperty.New("width", "100%")
					)
				)
			).ToContainerFragment();

				(new AllMeasurementsMustBePixels(
					AllMeasurementsMustBePixels.ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes,
					AllMeasurementsMustBePixels.RecommendedPercentageWidthExceptions
				)).EnsureRulesAreMet(new[] { content });
		}

		[Fact]
		public void HundredPercentageWidthImgsAreNotAcceptableIfNotNestedInPercentageWidthDivs()
		{
			var content = CSSFragmentBuilderSelector.New(
				"img",
				CSSFragmentBuilderStyleProperty.New("width", "100%")
			).ToContainerFragment();

			Assert.Throws<AllMeasurementsMustBePixels.AllMeasurementsMustBePixelsNotAppliedException>(() =>
			{
				(new AllMeasurementsMustBePixels(
					AllMeasurementsMustBePixels.ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes,
					AllMeasurementsMustBePixels.RecommendedPercentageWidthExceptions
				)).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void NonHundredPercentageWidthImgsAreNotAcceptableIfNestedInPercentageWidthDivs()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("width", "50%"),
				CSSFragmentBuilderSelector.New(
					"img",
					CSSFragmentBuilderStyleProperty.New("width", "80%")
				)
			).ToContainerFragment();

			Assert.Throws<AllMeasurementsMustBePixels.AllMeasurementsMustBePixelsNotAppliedException>(() =>
			{
				(new AllMeasurementsMustBePixels(
					AllMeasurementsMustBePixels.ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes,
					AllMeasurementsMustBePixels.RecommendedPercentageWidthExceptions
				)).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void CombinedBorderPropertyValueMayNotUseEms()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("border", "0.5em solid black")
			).ToContainerFragment();

			Assert.Throws<AllMeasurementsMustBePixels.AllMeasurementsMustBePixelsNotAppliedException>(() =>
			{
				(new AllMeasurementsMustBePixels(
					AllMeasurementsMustBePixels.ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes,
					AllMeasurementsMustBePixels.RecommendedPercentageWidthExceptions
				)).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void BorderMayNotSpecifyWidthAsThick()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("border", "thick solid black")
			).ToContainerFragment();

			Assert.Throws<AllMeasurementsMustBePixels.AllMeasurementsMustBePixelsNotAppliedException>(() =>
			{
				(new AllMeasurementsMustBePixels(
					AllMeasurementsMustBePixels.ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes,
					AllMeasurementsMustBePixels.RecommendedPercentageWidthExceptions
				)).EnsureRulesAreMet(new[] { content });
			});
		}

		/// <summary>
		/// Since auto is not a numeric value it is not checked to ensure that it has "pixel" units, since "0" doesn't have any units (and because it IS zero and
		/// not any other value) it is also allowed
		/// </summary>
		[Fact]
		public void MarginAutoIsAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("margin", "0 auto")
			).ToContainerFragment();

				(new AllMeasurementsMustBePixels(
					AllMeasurementsMustBePixels.ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes,
					AllMeasurementsMustBePixels.RecommendedPercentageWidthExceptions
				)).EnsureRulesAreMet(new[] { content });
		}

		/// <summary>
		/// percent(0.1) should be disallowed in the same places that 10% is (this is an way to describe percentage measurements that I wasn't previously aware of!)
		/// </summary>
		[Fact]
		public void PercentageKeywordWorkaroundIsCaught()
		{
			var content = CSSFragmentBuilderSelector.New(
				"p",
				CSSFragmentBuilderStyleProperty.New("width", "percentage(0.1)")
			).ToContainerFragment();

			Assert.Throws<AllMeasurementsMustBePixels.AllMeasurementsMustBePixelsNotAppliedException>(() =>
			{
				(new AllMeasurementsMustBePixels(
					AllMeasurementsMustBePixels.ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes,
					AllMeasurementsMustBePixels.RecommendedPercentageWidthExceptions
				)).EnsureRulesAreMet(new[] { content });
			});
		}

		/// <summary>
		/// When the AllowOneHundredPercentOnAnyElementAndProperty behaviour is specified then any element should be allowed to be set to 100% or 100% !important
		/// (the latter is being tested for explicitly here since a logic error previously was not allowing it)
		/// </summary>
		[Fact]
		public void HundredPercentWidthImportantIsAcceptedWhenAllowingAnyElementToBeHundredPercentWidth()
		{
			var content = CSSFragmentBuilderSelector.New(
				"a",
				CSSFragmentBuilderStyleProperty.New("width", "100% !important")
			).ToContainerFragment();

				(new AllMeasurementsMustBePixels(
					AllMeasurementsMustBePixels.ConformityOptions.AllowOneHundredPercentOnAnyElementAndProperty
				)).EnsureRulesAreMet(new[] { content });
		}


		[Fact]
		public void HundredPercentWidthImportantReturnsNoErrorsWhenAllowingAnyElementToBeHundredPercentWidth()
		{
			var content = CSSFragmentBuilderSelector.New(
				"a",
				CSSFragmentBuilderStyleProperty.New("width", "100% !important")
			).ToContainerFragment();

			Assert.Equal(0, (new AllMeasurementsMustBePixels(
				AllMeasurementsMustBePixels.ConformityOptions.AllowOneHundredPercentOnAnyElementAndProperty
			)).GetAnyBrokenRules(new[] { content }).Count());
		}

		/// <summary>
		/// This tests exists as an example against a report that 100% was not supported for these two properties
		/// </summary>
		[Fact]
		public void RecommendedConfigurationAllowsHundredPercentageWidthOnBackgroundPositionAndSize()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("background-size", "100%"),
				CSSFragmentBuilderStyleProperty.New("background-position", "100%")
			).ToContainerFragment();

			AllMeasurementsMustBePixels.Recommended.EnsureRulesAreMet(new[] { content });
		}

		/// <summary>
		/// This tests uses the original data from the report that resulted in RecommendedConfigurationAllowsHundredPercentageWidthOnBackgroundPositionAndSize (and this
		/// data did require a change)
		/// </summary>
		[Fact]
		public void RecommendedConfigurationAllowsHundredPercentageWidthOnBackgroundPositionAndSizeAsMultiPartValue()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("background-size", "100% -20px"),
				CSSFragmentBuilderStyleProperty.New("background-position", "100% 19px")
			).ToContainerFragment();

			AllMeasurementsMustBePixels.Recommended.EnsureRulesAreMet(new[] { content });
		}

		[Theory, MemberData("GetAnyBrokenRulesErrorCountContent")]
		public void GetAnyBrokenRulesErrorCount(int Id, ICSSFragment content, int expectedErrors)
		{
			Assert.Equal(
				expectedErrors,
				(new AllMeasurementsMustBePixels(
					AllMeasurementsMustBePixels.ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes,
					AllMeasurementsMustBePixels.RecommendedPercentageWidthExceptions)
				).GetAnyBrokenRules(new[] { content }).Count()
			);
		}

		public static IEnumerable<object[]> GetAnyBrokenRulesErrorCountContent
		{
			get
			{
				return new[]
				{
					new object[] {1,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("width", "50%")).ToContainerFragment(),0 },
					new object[] {2,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("width", "50%"),CSSFragmentBuilderSelector.New("img",CSSFragmentBuilderStyleProperty.New("width", "100%"))).ToContainerFragment(),0},
					new object[] {3,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("width", "50%"),CSSFragmentBuilderSelector.New("p",CSSFragmentBuilderSelector.New("img",CSSFragmentBuilderStyleProperty.New("width", "100%")))).ToContainerFragment(),0},
					new object[] {4,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("width", "50%"),CSSFragmentBuilderSelector.New("p",CSSFragmentBuilderSelector.New("img",CSSFragmentBuilderStyleProperty.New("width", "100%")))).ToContainerFragment(),0},
					new object[] {5,CSSFragmentBuilderSelector.New("img",CSSFragmentBuilderStyleProperty.New("width", "100%")).ToContainerFragment(),1},
					new object[] {6,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("width", "50%"),CSSFragmentBuilderSelector.New("img",CSSFragmentBuilderStyleProperty.New("width", "80%"))).ToContainerFragment(),1},
					new object[] {7,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("border", "0.5em solid black")).ToContainerFragment() ,1},
					new object[] {8,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("border", "thick solid black")).ToContainerFragment() ,1},
					new object[] {9,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("margin", "0 auto")).ToContainerFragment() ,0},
					new object[] {10,CSSFragmentBuilderSelector.New("p",CSSFragmentBuilderStyleProperty.New("width", "percentage(0.1)")).ToContainerFragment() ,1},
					new object[] {11,CSSFragmentBuilderSelector.New("a",CSSFragmentBuilderStyleProperty.New("width", "100% !important")).ToContainerFragment() ,1},
					new object[] {12,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("margin", "10pt 10pt 15px 0;")).ToContainerFragment() ,1}
				};
			}
		}
	}
}
