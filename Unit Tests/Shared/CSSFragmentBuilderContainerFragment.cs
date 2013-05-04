using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSSParser;
using CSSParser.ExtendedLESSParser;

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

			Selectors = new ContainerFragment.SelectorSet(
				selectors.Split(',').Select(selector => new ContainerFragment.WhiteSpaceNormalisedString(selector))
			);
			Children = childrenList.AsReadOnly();
		}

		/// <summary>
		/// This will never be null
		/// </summary>
		public ContainerFragment.SelectorSet Selectors { get; private set; }

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

		public ContainerFragment ToContainerFragment()
		{
			var translatedData = Translate(this, new ContainerFragment.SelectorSet[0]);
			if (translatedData == null)
				throw new Exception("Got null back from the Translate method!");
			var translatedDataArray = translatedData.ToArray();
			if (translatedDataArray.Length != 1)
				throw new Exception("Should only have got one fragment back from the Translate method for the CSSFragmentBuilderSelector!");
			if (!(translatedDataArray[0] is ContainerFragment))
				throw new Exception("Should only have got one fragment back from the Translate method for the CSSFragmentBuilderSelector; a ContainerFragment!");
			return (ContainerFragment)translatedDataArray[0];
		}

		private static IEnumerable<ICSSFragment> Translate(ICSSFragmentBuilderComponent source, IEnumerable<ContainerFragment.SelectorSet> parentSelectors)
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
				var stylePropertyName = new StylePropertyName(styleProperty.Name, 0);
				return new ICSSFragment[]
				{
					stylePropertyName,
					new StylePropertyValue(
						stylePropertyName,
						GetValueSections(styleProperty.Value),
						0
					)
				};
			}

			var selectorProperty = source as CSSFragmentBuilderSelector;
			if (selectorProperty != null)
			{
				var selectors = selectorProperty.Selectors;
				var childFragments = selectorProperty.Children.SelectMany(c => Translate(c, parentSelectors.Concat(new[] { selectorProperty.Selectors })));
				ICSSFragment newFragment; 
				if (selectors.First().Value.StartsWith("@media", StringComparison.InvariantCultureIgnoreCase))
					newFragment = new MediaQuery(selectors, parentSelectors, 0, childFragments);
				else
					newFragment = new Selector(selectors, parentSelectors, 0, childFragments);
				return new[] { newFragment };
			}

			throw new ArgumentException("Unsupported type: " + source.GetType());
		}

		private static IEnumerable<string> GetValueSections(string value)
		{
			if (value == null)
				throw new ArgumentNullException("value");

			// The CSSParser has to deal with quoting of values so we can use it here - given a property value it should return a set of sections where the only
			// CharacterCategorisation values are Whitespace and Value, any whitespace within a quoted value will be identified as Value, not Whitespace
			var sections = new List<string>();
			var buffer = new StringBuilder();
			foreach (var section in Parser.ParseCSS(value))
			{
				if (section.CharacterCategorisation == CSSParser.ContentProcessors.CharacterCategorisationOptions.Whitespace)
				{
					if (buffer.Length > 0)
					{
						sections.Add(buffer.ToString());
						buffer.Clear();
					}
				}
				else
					buffer.Append(section.Value);
			}
			if (buffer.Length > 0)
				sections.Add(buffer.ToString());
			return sections;
		}
	}
}
