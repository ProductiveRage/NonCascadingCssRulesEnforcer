using System;
using CSSParser.ExtendedLESSParser;
using NonCascadingCSSRulesEnforcer.Rules;
using UnitTests.Shared;
using Xunit;
using CSSParser.ExtendedLESSParser.ContentSections;

namespace UnitTests.Rules
{
	public class NoSelectorMayBeRepeatedInTheRulesTests
	{
		[Fact]
		public void NullInputWillThrowAnException()
		{
			var ruleEnforcer = new NoSelectorMayBeRepeatedInTheRules(
				NoSelectorMayBeRepeatedInTheRules.ConformityOptions.AllowBareSelectorsToBeRepeated
			);
			Assert.Throws<ArgumentNullException>(() =>
			{
				ruleEnforcer.EnsureRulesAreMet(null);
			});
		}

		[Fact]
		public void EmptyContentIsFine()
		{
			var ruleEnforcer = new NoSelectorMayBeRepeatedInTheRules(
				NoSelectorMayBeRepeatedInTheRules.ConformityOptions.AllowBareSelectorsToBeRepeated
			);
			Assert.DoesNotThrow(() =>
			{
				ruleEnforcer.EnsureRulesAreMet(new ICSSFragment[0]);
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

			var ruleEnforcer = new NoSelectorMayBeRepeatedInTheRules(
				NoSelectorMayBeRepeatedInTheRules.ConformityOptions.AllowBareSelectorsToBeRepeated
			);
			Assert.DoesNotThrow(() =>
			{
				ruleEnforcer.EnsureRulesAreMet(content);
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

			var ruleEnforcer = new NoSelectorMayBeRepeatedInTheRules(
				NoSelectorMayBeRepeatedInTheRules.ConformityOptions.AllowBareSelectorsToBeRepeated
			);
			Assert.Throws<NoSelectorMayBeRepeatedInTheRules.NoSelectorMayBeRepeatedInTheRulesException>(() =>
			{
				ruleEnforcer.EnsureRulesAreMet(content);
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

			var ruleEnforcer = new NoSelectorMayBeRepeatedInTheRules(
				NoSelectorMayBeRepeatedInTheRules.ConformityOptions.AllowBareSelectorsToBeRepeated
			);
			Assert.DoesNotThrow(() =>
			{
				ruleEnforcer.EnsureRulesAreMet(content);
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

			var ruleEnforcer = new NoSelectorMayBeRepeatedInTheRules(
				NoSelectorMayBeRepeatedInTheRules.ConformityOptions.AllowBareSelectorsToBeRepeated
			);
			Assert.DoesNotThrow(() =>
			{
				ruleEnforcer.EnsureRulesAreMet(content);
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

			var ruleEnforcer = new NoSelectorMayBeRepeatedInTheRules(
				NoSelectorMayBeRepeatedInTheRules.ConformityOptions.AllowBareSelectorsToBeRepeated
			);
			Assert.DoesNotThrow(() =>
			{
				ruleEnforcer.EnsureRulesAreMet(content);
			});
		}
	}
}
