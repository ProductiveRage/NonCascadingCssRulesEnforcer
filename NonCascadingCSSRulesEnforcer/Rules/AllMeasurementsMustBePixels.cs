using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CSSParser.ExtendedLESSParser.ContentSections;
using NonCascadingCSSRulesEnforcer.ExtendedLESSParserExtensions;

namespace NonCascadingCSSRulesEnforcer.Rules
{
	/// <summary>
	/// This rule applies to all stylesheets. Measurements all be specified in pixels (no use of em, rem, mm, etc..). Note that these units ARE allowed in media queries but not
	/// in styles for elements. This rule can be relaxed to allow certain elements that have percentage widths specified and for any images whose styles are nested within styles
	/// for those elements to have width 100%.
	/// If it is relaxed then it is recommended that only div, th and td elements are allowed percentage width. The first because a div element may be a container element used
	/// for layout (in HTML5 divs are still appropriate as containers if no semantic meaning is attached -see http://webdesign.about.com/od/html5tags/a/when-to-use-section-element.htm)
	/// while table cell elements do not abide by the same rules for layout that other elements do, it's valid to arrange the columns and fit the data around that rather than trying
	/// to fit layout around content. Also see http://www.productiverage.com/Read/46 for more justification of the relaxation of this rule.
	/// </summary>
	public class AllMeasurementsMustBePixels : IEnforceRules
	{
		private readonly ConformityOptions _conformity;
		private readonly IEnumerable<string> _percentageWidthElementTypesIfEnabled;
		public AllMeasurementsMustBePixels(ConformityOptions conformity, IEnumerable<string> percentageWidthElementTypesIfEnabled)
		{
			var allCombinedConformityOptionValues = 0;
			foreach (int conformityOptionValue in Enum.GetValues(typeof(ConformityOptions)))
				allCombinedConformityOptionValues = allCombinedConformityOptionValues | conformityOptionValue;
			if ((allCombinedConformityOptionValues | (int)conformity) != allCombinedConformityOptionValues)
				throw new ArgumentOutOfRangeException("conformity");

			_percentageWidthElementTypesIfEnabled = (percentageWidthElementTypesIfEnabled ?? new string[0])
				.Select(tagName => tagName.Trim().ToUpper())
				.Distinct()
				.ToArray();
			if (_percentageWidthElementTypesIfEnabled.Any(tagName => tagName == ""))
				throw new ArgumentException("Null/blank entry encountered in percentageWidthElementTypesIfEnabled set");

			if ((conformity & ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes) == ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes)
			{
				if (!_percentageWidthElementTypesIfEnabled.Any())
					throw new ArgumentException("percentageWidthElementTypesIfEnabled must have at least one value if AllowPercentageWidthsOnSpecifiedElementTypes is enabled");
			}
			else if (_percentageWidthElementTypesIfEnabled.Any())
				throw new ArgumentException("percentageWidthElementTypesIfEnabled may not have any values specified if AllowPercentageWidthsOnSpecifiedElementTypes is not enabled");

			_conformity = conformity;
		}
		public AllMeasurementsMustBePixels(ConformityOptions conformity) : this(conformity, new string[0]) { }

		/// <summary>
		/// If the ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes behaviour is enabled then these are the recommended exceptions: div, td and th
		/// </summary>
		public static IEnumerable<string> RecommendedPercentageWidthExceptions { get { return new[] { "div", "td", "th" }; } }

		public bool DoesThisRuleApplyTo(StyleSheetTypeOptions styleSheetType)
		{
			if (!Enum.IsDefined(typeof(StyleSheetTypeOptions), styleSheetType))
				throw new ArgumentOutOfRangeException("styleSheetType");

			// This can't be applied to compiled stylesheets as the ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypesntageWidthDivs option specifies that img elements
			// are allowed width:100% if the style is nested within a div that has percentage width (when the rules are compiled this nesting will no longer be possible)
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
			/// This will specified element types to have a width with a percentage unit and img elements whose styles are nested within their style blocks to have width:100%,
			/// the recommended element types for this option are div, td and th - this set is exposed through the static RecommendedPercentageWidthExceptions property
			/// </summary>
			AllowPercentageWidthsOnSpecifiedElementTypes = 1,

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
					var stylePropertyValueFragmentCombinedSegments = string.Join(" ", stylePropertyValueFragment.ValueSegments);
					if ((stylePropertyValueFragmentCombinedSegments == "100%")
					|| stylePropertyValueFragmentCombinedSegments.Equals("100% !important", StringComparison.OrdinalIgnoreCase))
						continue;
				}
				foreach (var value in stylePropertyValueFragmentSections)
				{
					if ((_conformity & ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes) == ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes)
					{
						// To get the parent Selector we have to walk backwards up the containers set since this property value could be wrapped in a MediaQuery - eg.
						// div.Whatever {
						//   @media screen and (max-width:70em) {
						//     width: 50%;
						//   }
						// }
						var directParentSelector = containers.LastOrDefault(c => c is Selector) as Selector;
						if ((directParentSelector != null) && stylePropertyValueFragment.Property.Value.Equals("width", StringComparison.InvariantCultureIgnoreCase) && IsPercentageMeasurement(value))
						{
							// If the selector for this property targets divs or tds only (eg. "div.Main" or "div.Header div.Logo, div.Footer div.Logo") then allow
							// percentage widths
							if (DoesSelectorTargetOnlyElementsWithTagNames(directParentSelector, _percentageWidthElementTypesIfEnabled))
								continue;

							// If the selector for this property targets imgs only then allow "width:100%" values so long as they are inside a div or td with a percentage width
							if (DoesSelectorTargetOnlyElementsWithTagNames(directParentSelector, new[] { "img" }))
							{
								if (IsPercentageMeasurement(value))
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
					
					// Check for measurements that are a numeric value and a unit string, where the unit string is any other than "px"
					foreach (var disallowedMeasurementUnit in DisallowedMeasurementUnits)
					{
						if (!value.EndsWith(disallowedMeasurementUnit, StringComparison.InvariantCultureIgnoreCase))
							continue;

						float numericValue;
						if (float.TryParse(value.Substring(0, value.Length - disallowedMeasurementUnit.Length).Trim(), out numericValue))
							throw new AllMeasurementsMustBePixelsNotAppliedException(fragment);
					}

					// If the measurement is a percentage that wasn't caught above then either it's not valid or it uses the "percentage(0.1)" format, either way
					// it's not allowed at this point
					if (IsPercentageMeasurement(value))
						throw new AllMeasurementsMustBePixelsNotAppliedException(fragment);
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

		private bool IsPercentageMeasurement(string value)
		{
			if (value == null)
				throw new ArgumentNullException("value");

			return value.EndsWith("%") || value.StartsWith("percentage(", StringComparison.InvariantCultureIgnoreCase);
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
