using System;
using NonCascadingCSSRulesEnforcer.Rules;
using UnitTests.Shared;
using Xunit;
using CSSParser.ExtendedLESSParser;

namespace UnitTests.Rules
{
	public class NoSelectorMayBeRepeatedInTheRulesTests
	{
		[Fact]
		public void NullInputWillThrowAnException()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				(new NoSelectorMayBeRepeatedInTheRules()).EnsureRulesAreMet(null);
			});
		}

		[Fact]
		public void EmptyContentIsFine()
		{
			Assert.DoesNotThrow(() =>
			{
				(new NoSelectorMayBeRepeatedInTheRules()).EnsureRulesAreMet(new ICSSFragment[0]);
			});
		}

		[Fact]
		public void SinglePropertyIsFine()
		{
			var content = new[]
			{
				CSSFragmentBuilderSelector.New(
					"div.Header",
					CSSFragmentBuilderStyleProperty.New("color", "red")
				).ToContainerFragment()
			};

			Assert.DoesNotThrow(() =>
			{
				(new NoSelectorMayBeRepeatedInTheRules()).EnsureRulesAreMet(content);
			});
		}

		[Fact]
		public void RepeatedSelectorsWithDifferentStylesIsNotAcceptable()
		{
			var content = new[]
			{
				CSSFragmentBuilderSelector.New(
					"div.Header",
					CSSFragmentBuilderStyleProperty.New("color", "red")
				).ToContainerFragment(),
				CSSFragmentBuilderSelector.New(
					"div.Header",
					CSSFragmentBuilderStyleProperty.New("color", "blue")
				).ToContainerFragment()
			};

			Assert.Throws<NoSelectorMayBeRepeatedInTheRules.NoSelectorMayBeRepeatedInTheRulesException>(() =>
			{
				(new NoSelectorMayBeRepeatedInTheRules()).EnsureRulesAreMet(content);
			});
		}

		[Fact]
		public void MultipleSelectorsThatAreDifferentAreFine()
		{
			var content = new[]
			{
				CSSFragmentBuilderSelector.New(
					"div.Header",
					CSSFragmentBuilderStyleProperty.New("color", "red")
				).ToContainerFragment(),
				CSSFragmentBuilderSelector.New(
					"div.Footer",
					CSSFragmentBuilderStyleProperty.New("color", "blue")
				).ToContainerFragment()
			};

			Assert.DoesNotThrow(() =>
			{
				(new NoSelectorMayBeRepeatedInTheRules()).EnsureRulesAreMet(content);
			});
		}

		[Fact]
		public void RepeatedSelectorAppearingInDifferentMediaQueriesIsFine()
		{
			var content = new[]
			{
				CSSFragmentBuilderSelector.New(
					"div.Header",
					CSSFragmentBuilderStyleProperty.New("color", "red")
				).ToContainerFragment(),
				CSSFragmentBuilderSelector.New(
					"@media screen and (max-width:70em)",
					CSSFragmentBuilderSelector.New(
						"div.Header",
						CSSFragmentBuilderStyleProperty.New("color", "blue")
					)
				).ToContainerFragment()
			};

			Assert.DoesNotThrow(() =>
			{
				(new NoSelectorMayBeRepeatedInTheRules()).EnsureRulesAreMet(content);
			});
		}

		[Fact]
		public void RepeatedSelectorAppearingInDifferentNestedMediaQueriesIsFine()
		{
			var content = new[]
			{
				CSSFragmentBuilderSelector.New(
					"div.Header",
					CSSFragmentBuilderStyleProperty.New("color", "red")
				).ToContainerFragment(),
				CSSFragmentBuilderSelector.New(
					"@media screen and (max-width:70em)",
					CSSFragmentBuilderSelector.New(
						"div.Header",
						CSSFragmentBuilderStyleProperty.New("color", "blue"),
						CSSFragmentBuilderSelector.New(
							"@media screen and (max-width:30em)",
							CSSFragmentBuilderSelector.New(
								"div.Header",
								CSSFragmentBuilderStyleProperty.New("color", "yellow")
							)
						)
					)
				).ToContainerFragment()
			};

			Assert.DoesNotThrow(() =>
			{
				(new NoSelectorMayBeRepeatedInTheRules()).EnsureRulesAreMet(content);
			});
		}
	}
}
