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

			Assert.DoesNotThrow(() =>
			{
				(new AllMeasurementsMustBePixels(
					AllMeasurementsMustBePixels.ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes,
					AllMeasurementsMustBePixels.RecommendedPercentageWidthExceptions
				)).EnsureRulesAreMet(new[] { content });
			});
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

			Assert.DoesNotThrow(() =>
			{
				(new AllMeasurementsMustBePixels(
					AllMeasurementsMustBePixels.ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes,
					AllMeasurementsMustBePixels.RecommendedPercentageWidthExceptions
				)).EnsureRulesAreMet(new[] { content });
			});
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

			Assert.DoesNotThrow(() =>
			{
				(new AllMeasurementsMustBePixels(
					AllMeasurementsMustBePixels.ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes,
					AllMeasurementsMustBePixels.RecommendedPercentageWidthExceptions
				)).EnsureRulesAreMet(new[] { content });
			});
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

			Assert.DoesNotThrow(() =>
			{
				(new AllMeasurementsMustBePixels(
					AllMeasurementsMustBePixels.ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes,
					AllMeasurementsMustBePixels.RecommendedPercentageWidthExceptions
				)).EnsureRulesAreMet(new[] { content });
			});
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

			Assert.DoesNotThrow(() =>
			{
				(new AllMeasurementsMustBePixels(
					AllMeasurementsMustBePixels.ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes,
					AllMeasurementsMustBePixels.RecommendedPercentageWidthExceptions
				)).EnsureRulesAreMet(new[] { content });
			});
		}
	}
}
