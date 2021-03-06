﻿using System.Collections.Generic;
using System.Linq;
using CSSParser.ExtendedLESSParser.ContentSections;
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

			PaddingMustBeFullySpecifiedIfSpecifiedAtAll.Instance.EnsureRulesAreMet(new[] { content });
		}

		[Fact]
		public void PaddingFullyDefinedIsAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("padding", "16px")
			).ToContainerFragment();

			PaddingMustBeFullySpecifiedIfSpecifiedAtAll.Instance.EnsureRulesAreMet(new[] { content });
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

			PaddingMustBeFullySpecifiedIfSpecifiedAtAll.Instance.EnsureRulesAreMet(new[] { content });
		}

		[Fact]
		public void CombinationOfPaddingAndPaddingTopIsAcceptable()
		{
			var content = CSSFragmentBuilderSelector.New(
				"div",
				CSSFragmentBuilderStyleProperty.New("padding-top", "8px"),
				CSSFragmentBuilderStyleProperty.New("padding", "16px")
			).ToContainerFragment();

			PaddingMustBeFullySpecifiedIfSpecifiedAtAll.Instance.EnsureRulesAreMet(new[] { content });
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
				PaddingMustBeFullySpecifiedIfSpecifiedAtAll.Instance.EnsureRulesAreMet(new[] { content });
			});
		}

		[Theory, MemberData("GetAnyBrokenRulesContent")]
		public void GetAnyBrokenRulesCount(int Id, ICSSFragment content, int expectedErrors)
		{
			Assert.Equal(
				expectedErrors,
				PaddingMustBeFullySpecifiedIfSpecifiedAtAll.Instance.GetAnyBrokenRules(new[] { content }).Count()
				);
		}

		public static IEnumerable<object[]> GetAnyBrokenRulesContent
		{
			get
			{
				return new[]
				{
				new object[] {1,CSSFragmentBuilderSelector.New("div").ToContainerFragment(),0 },
				new object[] {2,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("padding", "16px")).ToContainerFragment(),0},
				new object[] {3,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("padding-top", "16px"),CSSFragmentBuilderStyleProperty.New("padding-left", "16px"),CSSFragmentBuilderStyleProperty.New("padding-bottom", "16px"),CSSFragmentBuilderStyleProperty.New("padding-right", "16px")).ToContainerFragment(),0},
				new object[] {4,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("padding-top", "8px"),CSSFragmentBuilderStyleProperty.New("padding", "16px")).ToContainerFragment(),0},
				new object[] {5,CSSFragmentBuilderSelector.New("div",CSSFragmentBuilderStyleProperty.New("padding-top", "16px")).ToContainerFragment(),1},
				};
			}
		}
	}
}
