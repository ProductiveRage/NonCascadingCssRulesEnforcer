﻿using System.Collections.Generic;
using System.Linq;
using CSSParser.ExtendedLESSParser.ContentSections;
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

			MarginMustBeFullySpecifiedIfSpecifiedAtAll.Instance.EnsureRulesAreMet(new[] { content });
		}

		[Fact]
		public void MarginFullyDefinedIsAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("margin", "16px")
			).ToContainerFragment();

			MarginMustBeFullySpecifiedIfSpecifiedAtAll.Instance.EnsureRulesAreMet(new[] { content });
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

			MarginMustBeFullySpecifiedIfSpecifiedAtAll.Instance.EnsureRulesAreMet(new[] { content });
		}

		[Fact]
		public void CombinationOfMarginAndMarginTopIsAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("margin-top", "8px"),
				CSSFragmentBuilderStyleProperty.New("margin", "16px")
			).ToContainerFragment();

			MarginMustBeFullySpecifiedIfSpecifiedAtAll.Instance.EnsureRulesAreMet(new[] { content });
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
				MarginMustBeFullySpecifiedIfSpecifiedAtAll.Instance.EnsureRulesAreMet(new[] { content });
			});
		}

		[Theory, MemberData("GetAnyBrokenRulesContent")]
		public void GetAnyBrokenRulesCount(int Id, ICSSFragment content, int expectedErrors)
		{
			Assert.Equal(
				expectedErrors,
				MarginMustBeFullySpecifiedIfSpecifiedAtAll.Instance.GetAnyBrokenRules(new[] { content }).Count()
			);
		}

		public static IEnumerable<object[]> GetAnyBrokenRulesContent
		{
			get
			{
				return new[]
				{
					new object[] {1,CSSFragmentBuilderSelector.New("div").ToContainerFragment(),0 },
					new object[] {2,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("margin", "16px")).ToContainerFragment(),0},
					new object[] {3,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("margin-top", "16px"),CSSFragmentBuilderStyleProperty.New("margin-left", "16px"),CSSFragmentBuilderStyleProperty.New("margin-bottom", "16px"),CSSFragmentBuilderStyleProperty.New("margin-right", "16px")).ToContainerFragment(),0},
					new object[] {4,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("margin-top", "8px"),CSSFragmentBuilderStyleProperty.New("margin", "16px")).ToContainerFragment(),0},
					new object[] {5,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("margin-top", "16px")).ToContainerFragment(),1},
				};
			}
		}
	}
}
