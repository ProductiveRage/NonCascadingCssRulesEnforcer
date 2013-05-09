using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CSSParser.ExtendedLESSParser;
using NonCascadingCSSRulesEnforcer.ExtendedLESSParserExtensions;

namespace NonCascadingCSSRulesEnforcer.Rules
{
	/// <summary>
	/// This rule applies to all stylesheets. Measurements all be specified in pixels (no use of em, rem, mm, etc..). Note that these units ARE allowed in media queries but not
	/// in styles for elements.
	/// </summary>
	public class AllMeasurementsMustBePixels : IEnforceRules
	{
		private readonly ConformityOptions _conformity;
		public AllMeasurementsMustBePixels(ConformityOptions conformity)
		{
			var allCombinedConformityOptionValues = 0;
			foreach (int conformityOptionValue in Enum.GetValues(typeof(ConformityOptions)))
				allCombinedConformityOptionValues = allCombinedConformityOptionValues | conformityOptionValue;
			if ((allCombinedConformityOptionValues | (int)conformity) != allCombinedConformityOptionValues)
				throw new ArgumentOutOfRangeException("conformity");

			_conformity = conformity;
		}

		public bool DoesThisRuleApplyTo(StyleSheetTypeOptions styleSheetType)
		{
			if (!Enum.IsDefined(typeof(StyleSheetTypeOptions), styleSheetType))
				throw new ArgumentOutOfRangeException("styleSheetType");

			// This can't be applied to compiled stylesheets as the ConformityOptions.AllowPercentageWidthDivs option specifies that img elements are allowed width:100% if
			// the style is nested within a div that has percentage width (when the rules are compiled this nesting will no longer be possible)
			return (styleSheetType != StyleSheetTypeOptions.Compiled);
		}

		[Flags]
		public enum ConformityOptions
		{
			/// <summary>
			/// This will not allow any element to have any measurement unit that is not in pixels
			/// </summary>
			Strict = 0,

			/// <summary>
			/// This will allow div and td elements to have a width with a percentage unit and img elements whose styles are nested within their style blocks to have width:100%
			/// </summary>
			AllowPercentageWidthDivsAndTDs = 1,

			/// <summary>
			/// This will allow any property to be specified as 100% (acceptable for width or font-size, for example)
			/// </summary>
			AllowOneHundredPercentOnAnyElementAndProperty = 2
		}

		/// <summary>
		/// This will throw an exception if the specified rule BrokenRuleEncounteredException is broken. It will throw an ArgumentException for a null fragments
		/// references, or one which contains a null reference.
		/// </summary>
		public void EnsureRulesAreMet(IEnumerable<ICSSFragment> fragments)
		{
			if (fragments == null)
				throw new ArgumentNullException("fragments");

			EnsureRulesAreMet(fragments, new ContainerFragment[0]);
		}

		private void EnsureRulesAreMet(IEnumerable<ICSSFragment> fragments, IEnumerable<ContainerFragment> containers)
		{
			if (fragments == null)
				throw new ArgumentNullException("fragments");
			if (containers == null)
				throw new ArgumentNullException("containers");

			foreach (var fragment in fragments)
			{
				if (fragment == null)
					throw new ArgumentException("Null reference encountered in fragments set");

				var containerFragment = fragment as ContainerFragment;
				if (containerFragment != null)
				{
					EnsureRulesAreMet(containerFragment.ChildFragments, containers.Concat(new[] { containerFragment }));
					continue;
				}

				var stylePropertyValueFragment = fragment as StylePropertyValue;
				if (stylePropertyValueFragment == null)
					continue;

				// Generic tests for measurement units
				var stylePropertyValueFragmentSections = stylePropertyValueFragment.ValueSegments;
				if ((_conformity & ConformityOptions.AllowOneHundredPercentOnAnyElementAndProperty) == ConformityOptions.AllowOneHundredPercentOnAnyElementAndProperty)
				{
					if (stylePropertyValueFragment.HasValue("100%"))
						continue;
				}
				foreach (var value in stylePropertyValueFragmentSections)
				{
					if ((_conformity & ConformityOptions.AllowPercentageWidthDivsAndTDs) == ConformityOptions.AllowPercentageWidthDivsAndTDs)
					{
						// If ConformityOptions.AllowPercentageWidthDivsAndTDs was specified then allow div and td elements to have a percentage width and for img elements
						// within those elements to have a width applied of 100% (the style must be declared nested within the div or td style as supported by LESS, it is
						// not sufficient for the style for the img to appear elsewhere as the rule will not be able to confirm that it is a descendent of a 100% div / td)
						// - See http://www.productiverage.com/Read/46 for justification for allowing the relaxation of this rule
						// - The HTML5 "section" element should have semantic meaning, "div" is still appropriate as a container if no semantic meaning is attached, as such
						//   it is acceptable that only div wrappers may have "width:100%" (see http://webdesign.about.com/od/html5tags/a/when-to-use-section-element.htm
						//   and http://html5doctor.com/you-can-still-use-div)
						// - When using tables, it is acceptable to specify a width since the normal rules about layout can be considered differently for tables, it's
						//   valid to arrange the columns and fit the data around that rather than trying to fit layout around content

						// To get the parent Selector we have to walk backwards up the containers set since this property value could be wrapped in a MediaQuery - eg.
						// div.Whatever {
						//   @media screen and (max-width:70em) {
						//     width: 50%;
						//   }
						// }
						var directParentSelector = containers.LastOrDefault(c => c is Selector) as Selector;
						if ((directParentSelector != null)
						&& stylePropertyValueFragment.Property.Value.Equals("width", StringComparison.InvariantCultureIgnoreCase)
						&& value.EndsWith("%", StringComparison.InvariantCultureIgnoreCase))
						{
							// If the selector for this property targets divs or tds only (eg. "div.Main" or "div.Header div.Logo, div.Footer div.Logo") then allow
							// percentage widths
							if (DoesSelectorTargetOnlyElementsWithTagNames(directParentSelector, new[] { "div", "td" }))
								continue;

							// If the selector for this property targets imgs only then allow "width:100%" values so long as they are inside a div or td with a percentage width
							if (DoesSelectorTargetOnlyElementsWithTagNames(directParentSelector, new[] { "img" }))
							{
								if (value.EndsWith("%"))
								{
									if (value != "100%")
										throw new AllMeasurementsMustBePixelsNotAppliedException("The only allow percentage width for img is 100%", fragment);

									// We only need to ensure that the img is nested within an element with a percentage width, we don't have to worry about ensuring that
									// the selector targets div/tds elements only since this is handled by the above check (that percentage-width styles only target divs)
									// - Technically this would allow "div.Content { width: 50%; img { width: 100%; img { width: 100%; } } }" but that's not an error case
									//   since the img tags are still 100% and inside a div with percentage width (it's probably not valid for img tags to be nested but
									//   that's not a concern for this class)
									var firstContainerWithPercentageWidth = containers
										.TakeWhile(c => c != directParentSelector)
										.SelectMany(s => s.ChildFragments)
										.Where(f => f is StylePropertyValue)
										.Cast<StylePropertyValue>()
										.Where(s => s.Property.HasName("width") && s.GetValueSectionsThatAreMeasurements().All(v => v.Unit =="%"))
										.FirstOrDefault();
									if (firstContainerWithPercentageWidth != null)
										continue;

									throw new AllMeasurementsMustBePixelsNotAppliedException(
										"Percentage width for img may is only allowable if nested within a div or td style with percentage width (and the img must have width:100%)",
										fragment
									);
								}
							}
						}
					}
					foreach (var disallowedMeasurementUnit in DisallowedMeasurementUnits)
					{
						if (!value.EndsWith(disallowedMeasurementUnit, StringComparison.InvariantCultureIgnoreCase))
							continue;

						float numericValue;
						if (float.TryParse(value.Substring(0, value.Length - disallowedMeasurementUnit.Length).Trim(), out numericValue))
							throw new AllMeasurementsMustBePixelsNotAppliedException(fragment);
					}
				}

				// Specific tests for disallowed measurement types
				// - Border widths must be explicitly specified, use of "thin", "medium" or "thick" are not allowed
				var stylePropertyNameValue = stylePropertyValueFragment.Property.Value.ToLower();
				if ((stylePropertyNameValue == "border") || stylePropertyNameValue.StartsWith("border-"))
				{
					if (stylePropertyValueFragmentSections.Any(s =>
						s.Equals("thin", StringComparison.InvariantCultureIgnoreCase) ||
						s.Equals("medium", StringComparison.InvariantCultureIgnoreCase) ||
						s.Equals("thick", StringComparison.InvariantCultureIgnoreCase)
					))
					{
						throw new AllMeasurementsMustBePixelsNotAppliedException(fragment);
					}
				}
			}
		}

		private static readonly string[] DisallowedMeasurementUnits = Constants.MeasurementUnits.Except(new[] { "px" }).ToArray();

		private bool DoesSelectorTargetOnlyElementsWithTagNames(Selector parentSelector, IEnumerable<string> tagNames)
		{
			if (parentSelector == null)
				throw new ArgumentNullException("parentSelector");
			if (tagNames == null)
				throw new ArgumentNullException("tagNames");

			var tagNamesTidied = tagNames.Select(t => (t ?? "").Trim()).ToArray();
			if (tagNamesTidied.Any(t => t == ""))
				throw new ArgumentException("Null/blank entry encountered in tagNames set");

			foreach (var finalSelectorSegment in parentSelector.Selectors.Select(s => s.Value.Split(' ').Last()))
			{
				var targetedTagName = finalSelectorSegment.Split(new[] { '.', '#', ':' }).First(); // TODO: Handle '[' and add unit test to illustrate
				if (!tagNamesTidied.Any(t => t.Equals(targetedTagName, StringComparison.InvariantCultureIgnoreCase)))
					return false;
			}
			return true;
		}

		public class AllMeasurementsMustBePixelsNotAppliedException : BrokenRuleEncounteredException
		{
			public AllMeasurementsMustBePixelsNotAppliedException(ICSSFragment fragment) : base("Measurement encountered that was not in pixels", fragment) { }
			public AllMeasurementsMustBePixelsNotAppliedException(string additionalMessageContent, ICSSFragment fragment)
				: base("Measurement encountered that was not in pixels: " + (additionalMessageContent ?? ""), fragment)
			{
				if (string.IsNullOrWhiteSpace(additionalMessageContent))
					throw new ArgumentException("Null/blank additionalMessageContent specified");
			}
			protected AllMeasurementsMustBePixelsNotAppliedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		}
	}
}
