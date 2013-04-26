using NonCascadingCSSRulesEnforcer.Rules;
using UnitTests.Shared;
using Xunit;

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

			Assert.DoesNotThrow(() =>
			{
				(new BorderAndPaddingMayNotBeCombinedWithWidth()).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void PaddingWithWidthAutoIsFine()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("padding", "16px"),
				CSSFragmentBuilderStyleProperty.New("width", "auto")
			).ToContainerFragment();

			Assert.DoesNotThrow(() =>
			{
				(new BorderAndPaddingMayNotBeCombinedWithWidth()).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void WidthOnlyIsFine()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("width", "320px")
			).ToContainerFragment();

			Assert.DoesNotThrow(() =>
			{
				(new BorderAndPaddingMayNotBeCombinedWithWidth()).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void WidthWithZeroPaddingIsFine()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("width", "320px"),
				CSSFragmentBuilderStyleProperty.New("padding", "0")
			).ToContainerFragment();

			Assert.DoesNotThrow(() =>
			{
				(new BorderAndPaddingMayNotBeCombinedWithWidth()).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void WidthWithZeroPixelPaddingIsFine()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("width", "320px"),
				CSSFragmentBuilderStyleProperty.New("padding", "0px")
			).ToContainerFragment();

			Assert.DoesNotThrow(() =>
			{
				(new BorderAndPaddingMayNotBeCombinedWithWidth()).EnsureRulesAreMet(new[] { content });
			});
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
				(new BorderAndPaddingMayNotBeCombinedWithWidth()).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void WidthWithBorderNoneIsFine()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("width", "320px"),
				CSSFragmentBuilderStyleProperty.New("border", "none")
			).ToContainerFragment();

			Assert.DoesNotThrow(() =>
			{
				(new BorderAndPaddingMayNotBeCombinedWithWidth()).EnsureRulesAreMet(new[] { content });
			});
		}

	}
}
