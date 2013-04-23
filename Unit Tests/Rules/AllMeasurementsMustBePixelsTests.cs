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
				(new AllMeasurementsMustBePixels(AllMeasurementsMustBePixels.ConformityOptions.AllowPercentageWidthDivs)).EnsureRulesAreMet(new[] { content });
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
				(new AllMeasurementsMustBePixels(AllMeasurementsMustBePixels.ConformityOptions.AllowPercentageWidthDivs)).EnsureRulesAreMet(new[] { content });
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
				(new AllMeasurementsMustBePixels(AllMeasurementsMustBePixels.ConformityOptions.AllowPercentageWidthDivs)).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void HundredPercentageWidthImgsAreNotAcceptableIfNotNestedInPercentageWidthDivs()
		{
			var content = CSSFragmentBuilderSelector.New(
				"img",
				CSSFragmentBuilderStyleProperty.New("width", "100%")
			).ToContainerFragment();

			Assert.Throws<AllMeasurementsMustBePixels.AllMeasurementsMustBePixelsNotAppliedException>(
				() => (new AllMeasurementsMustBePixels(AllMeasurementsMustBePixels.ConformityOptions.AllowPercentageWidthDivs)).EnsureRulesAreMet(new[] { content })
			);
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

			Assert.Throws<AllMeasurementsMustBePixels.AllMeasurementsMustBePixelsNotAppliedException>(
				() => (new AllMeasurementsMustBePixels(AllMeasurementsMustBePixels.ConformityOptions.AllowPercentageWidthDivs)).EnsureRulesAreMet(new[] { content })
			);
		}

		[Fact]
		public void CombinedBorderPropertyValueMayNotUseEms()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("border", "0.5em solid black")
			).ToContainerFragment();

			Assert.Throws<AllMeasurementsMustBePixels.AllMeasurementsMustBePixelsNotAppliedException>(
				() => (new AllMeasurementsMustBePixels(AllMeasurementsMustBePixels.ConformityOptions.AllowPercentageWidthDivs)).EnsureRulesAreMet(new[] { content })
			);
		}
	}
}
