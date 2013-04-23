using NonCascadingCSSRulesEnforcer.Rules;
using UnitTests.Shared;
using Xunit;

namespace UnitTests.Rules
{
	public class MarginMustBeFullySpecifiedIfSpecifiedAtAllTests
	{
		[Fact]
		public void NoMarginAtAllIsAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New("div").ToContainerFragment();

			Assert.DoesNotThrow(() =>
			{
				(new MarginMustBeFullySpecifiedIfSpecifiedAtAll()).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void MarginFullyDefinedIsAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("margin", "16px")
			).ToContainerFragment();

			Assert.DoesNotThrow(() =>
			{
				(new MarginMustBeFullySpecifiedIfSpecifiedAtAll()).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void MarginExplicitlyFullyDefinedIsAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("margin-top", "16px"),
				CSSFragmentBuilderStyleProperty.New("margin-left", "16px"),
				CSSFragmentBuilderStyleProperty.New("margin-bottom", "16px"),
				CSSFragmentBuilderStyleProperty.New("margin-right", "16px")
			).ToContainerFragment();

			Assert.DoesNotThrow(() =>
			{
				(new MarginMustBeFullySpecifiedIfSpecifiedAtAll()).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void CombinationOfMarginAndMarginTopIsAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("margin-top", "8px"),
				CSSFragmentBuilderStyleProperty.New("margin", "16px")
			).ToContainerFragment();

			Assert.DoesNotThrow(() =>
			{
				(new MarginMustBeFullySpecifiedIfSpecifiedAtAll()).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void MarginTopOnlyIsNotAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("margin-top", "16px")
			).ToContainerFragment();

			Assert.Throws<MarginMustBeFullySpecifiedIfSpecifiedAtAll.MarginMustBeFullySpecifiedIfSpecifiedAtAllException>(() =>
			{
				(new MarginMustBeFullySpecifiedIfSpecifiedAtAll()).EnsureRulesAreMet(new[] { content });
			});
		}
	}
}
