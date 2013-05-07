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
				(new BorderAndPaddingMayNotBeCombinedWithWidth(BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict)).EnsureRulesAreMet(new[] { content });
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
				(new BorderAndPaddingMayNotBeCombinedWithWidth(BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict)).EnsureRulesAreMet(new[] { content });
			});
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

			Assert.DoesNotThrow(() =>
			{
				(new BorderAndPaddingMayNotBeCombinedWithWidth(BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict)).EnsureRulesAreMet(new[] { content });
			});
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

			Assert.DoesNotThrow(() =>
			{
				(new BorderAndPaddingMayNotBeCombinedWithWidth(BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict)).EnsureRulesAreMet(new[] { content });
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
				(new BorderAndPaddingMayNotBeCombinedWithWidth(BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict)).EnsureRulesAreMet(new[] { content });
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
				(new BorderAndPaddingMayNotBeCombinedWithWidth(BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict)).EnsureRulesAreMet(new[] { content });
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
				(new BorderAndPaddingMayNotBeCombinedWithWidth(BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict)).EnsureRulesAreMet(new[] { content });
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
				(new BorderAndPaddingMayNotBeCombinedWithWidth(BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict)).EnsureRulesAreMet(new[] { content });
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
				(new BorderAndPaddingMayNotBeCombinedWithWidth(BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict)).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void WidthWithZeroWidthBorder()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("width", "320px"),
				CSSFragmentBuilderStyleProperty.New("border", "0 solid black")
			).ToContainerFragment();

			Assert.DoesNotThrow(() =>
			{
				(new BorderAndPaddingMayNotBeCombinedWithWidth(BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict)).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void WidthWithZeroPixelWidthBorder()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("width", "320px"),
				CSSFragmentBuilderStyleProperty.New("border", "0px solid black")
			).ToContainerFragment();

			Assert.DoesNotThrow(() =>
			{
				(new BorderAndPaddingMayNotBeCombinedWithWidth(BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict)).EnsureRulesAreMet(new[] { content });
			});
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

			Assert.DoesNotThrow(() =>
			{
				(new BorderAndPaddingMayNotBeCombinedWithWidth(BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.Strict)).EnsureRulesAreMet(new[] { content });
			});
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

			Assert.DoesNotThrow(() =>
			{
				(new BorderAndPaddingMayNotBeCombinedWithWidth(
					BorderAndPaddingMayNotBeCombinedWithWidth.ConformityOptions.AllowVerticalBorderAndPadding
				)).EnsureRulesAreMet(new[] { content });
			});
		}
	}
}
