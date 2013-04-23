using NonCascadingCSSRulesEnforcer.Rules;
using UnitTests.Shared;
using Xunit;

namespace UnitTests.Rules
{
	public class PaddingMustBeFullySpecifiedIfSpecifiedAtAllTests
	{
		[Fact]
		public void NoPaddingAtAllIsAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New("div").ToContainerFragment();

			Assert.DoesNotThrow(() =>
			{
				(new PaddingMustBeFullySpecifiedIfSpecifiedAtAll()).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void PaddingFullyDefinedIsAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("padding", "16px")
			).ToContainerFragment();

			Assert.DoesNotThrow(() =>
			{
				(new PaddingMustBeFullySpecifiedIfSpecifiedAtAll()).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void PaddingExplicitlyFullyDefinedIsAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("padding-top", "16px"),
				CSSFragmentBuilderStyleProperty.New("padding-left", "16px"),
				CSSFragmentBuilderStyleProperty.New("padding-bottom", "16px"),
				CSSFragmentBuilderStyleProperty.New("padding-right", "16px")
			).ToContainerFragment();

			Assert.DoesNotThrow(() =>
			{
				(new PaddingMustBeFullySpecifiedIfSpecifiedAtAll()).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void CombinationOfPaddingAndPaddingTopIsAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("padding-top", "8px"),
				CSSFragmentBuilderStyleProperty.New("padding", "16px")
			).ToContainerFragment();

			Assert.DoesNotThrow(() =>
			{
				(new PaddingMustBeFullySpecifiedIfSpecifiedAtAll()).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void PaddingTopOnlyIsNotAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("padding-top", "16px")
			).ToContainerFragment();

			Assert.Throws<PaddingMustBeFullySpecifiedIfSpecifiedAtAll.PaddingMustBeFullySpecifiedIfSpecifiedAtAllException>(() =>
			{
				(new PaddingMustBeFullySpecifiedIfSpecifiedAtAll()).EnsureRulesAreMet(new[] { content });
			});
		}
	}
}
