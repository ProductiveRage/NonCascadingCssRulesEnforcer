﻿using System;
using System.Collections.Generic;
using System.Linq;
using NonCascadingCSSRulesEnforcer.HierarchicalParsing;

namespace UnitTests.Shared
{
	public class CSSFragmentBuilderSelector : ICSSFragmentBuilderComponent
	{
		public static CSSFragmentBuilderSelector New(string selectors, params ICSSFragmentBuilderComponent[] children)
		{
			return new CSSFragmentBuilderSelector(selectors, children);
		}

		private CSSFragmentBuilderSelector(string selectors, IEnumerable<ICSSFragmentBuilderComponent> children)
		{
			if (string.IsNullOrWhiteSpace(selectors))
				throw new ArgumentException("Null/blank selectors specified");
			if (children == null)
				throw new ArgumentNullException("children");
			var childrenList = children.ToList();
			if (childrenList.Any(c => c == null))
				throw new ArgumentException("Null reference encountered in children set");

			Selectors = new Selector.SelectorSet(
				selectors.Split(',').Select(selector => new Selector.WhiteSpaceNormalisedString(selector))
			);
			Children = childrenList.AsReadOnly();
		}

		/// <summary>
		/// This will never be null
		/// </summary>
		public Selector.SelectorSet Selectors { get; private set; }

		/// <summary>
		/// This will never be null nor contain any nulls
		/// </summary>
		public IEnumerable<ICSSFragmentBuilderComponent> Children { get; private set; }

		public CSSFragmentBuilderSelector AddChildren(IEnumerable<ICSSFragmentBuilderComponent> children)
		{
			if (children == null)
				throw new ArgumentNullException("fragments");
			var childrenArray = children.ToArray();
			if (childrenArray.Any(f => f == null))
				throw new ArgumentException("Null reference encountered in children set");
			return new CSSFragmentBuilderSelector(
				string.Join(", ", Selectors.Select(s => s.Value)),
				Children.Concat(children)
			);
		}

		public CSSFragmentBuilderSelector AddChild(ICSSFragmentBuilderComponent child)
		{
			if (child == null)
				throw new ArgumentNullException("child");
			return AddChildren(new[] { child });
		}

		public Selector ToSelector()
		{
			var translatedData = Translate(this, new Selector.SelectorSet[0]);
			if (translatedData == null)
				throw new Exception("Got null back from the Translate method!");
			var translatedDataArray = translatedData.ToArray();
			if (translatedDataArray.Length != 1)
				throw new Exception("Should only have got one fragment back from the Translate method for the CSSFragmentBuilderSelector!");
			if (!(translatedDataArray[0] is Selector))
				throw new Exception("Should only have got one fragment back from the Translate method for the CSSFragmentBuilderSelector; a Selector!");
			return (Selector)translatedDataArray[0];
		}

		private static IEnumerable<ICSSFragment> Translate(ICSSFragmentBuilderComponent source, IEnumerable<Selector.SelectorSet> parentSelectors)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			if (parentSelectors == null)
				throw new ArgumentNullException("parentSelectors");

			var parentSelectorsArray = parentSelectors.ToArray();
			if (parentSelectorsArray.Any(s => s == null))
				throw new ArgumentException("Null reference encountered in parentSelectors set");

			var styleProperty = source as CSSFragmentBuilderStyleProperty;
			if (styleProperty != null)
			{
				return new ICSSFragment[]
				{
					new StylePropertyName(styleProperty.Name, 0),
					new StylePropertyValue(styleProperty.Value, 0)
				};
			}

			var selectorProperty = source as CSSFragmentBuilderSelector;
			if (selectorProperty != null)
			{
				return new ICSSFragment[]
				{
					new Selector(
						selectorProperty.Selectors,
						parentSelectors,
						0,
					selectorProperty.Children.SelectMany(c => Translate(c, parentSelectors.Concat(new[] { selectorProperty.Selectors })))
					)
				};
			}

			throw new ArgumentException("Unsupported type: " + source.GetType());
		}
	}
}