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
	/// in styles for elements. This rule can be relaxed to allow certain elements that have percentage widths specified (or any certain elements to have any percentage property
	/// specified ) and for any images whose styles are nested within styles for those elements to have width 100%.
	/// If it is relaxed then it is recommended that only div, li, th and td elements are allowed percentage properties. The first because a div element may be a container element used
	/// for layout (in HTML5 divs are still appropriate as containers if no semantic meaning is attached - see http://webdesign.about.com/od/html5tags/a/when-to-use-section-element.htm)
	/// while table cell elements do not abide by the same rules for layout that other elements do, it's valid to arrange the columns and fit the data around that rather than trying
	/// to fit layout around content. Also see http://www.productiverage.com/Read/46 for more justification of the relaxation of this rule.
	/// </summary>
	public class AllMeasurementsMustBePixels : IEnforceRules
	{
		/// <summary>
		/// The recommended configuration allows percentage properties on div, th, td and li elements since it's common to use them for layout - for your own use you may wish to be
		/// stricter or more relaxed (allow percentage widths on fewer or more elements). Using only pixel widths makes everything much more predictable but there are some layout
		/// elements which require percentage widths in order to be responsive (such as a horizontal gallery displaying four items at any one time). It also allows any element to
		/// have width 100% specified since that doesn't sacrifice any predictability.
		/// </summary>
		public static AllMeasurementsMustBePixels Recommended { get; } = new AllMeasurementsMustBePixels(
			ConformityOptions.AllowOneHundredPercentOnAnyElementAndProperty |
			ConformityOptions.AllowPercentagesOnAllPropertiesOfSpecifiedElementTypes |
			ConformityOptions.DoNotValidateKeyFramesProperties,
			new[] { "div", "td", "th", "li" }
		);

		private readonly ConformityOptions _conformity;
		private readonly IEnumerable<string> _percentageElementTypesIfEnabled;
		public AllMeasurementsMustBePixels(ConformityOptions conformity, IEnumerable<string> percentageElementTypesIfEnabled)
		{
			var allCombinedConformityOptionValues = 0;
			foreach (int conformityOptionValue in Enum.GetValues(typeof(ConformityOptions)))
				allCombinedConformityOptionValues = allCombinedConformityOptionValues | conformityOptionValue;
			if ((allCombinedConformityOptionValues | (int)conformity) != allCombinedConformityOptionValues)
				throw new ArgumentOutOfRangeException("conformity");

			_percentageElementTypesIfEnabled = (percentageElementTypesIfEnabled ?? new string[0])
				.Select(tagName => tagName.Trim().ToUpper())
				.Distinct()
				.ToArray();
			if (_percentageElementTypesIfEnabled.Any(tagName => tagName == ""))
				throw new ArgumentException("Null/blank entry encountered in percentageElementTypesIfEnabled set");

			if (conformity.HasFlag(ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes)
			|| conformity.HasFlag(ConformityOptions.AllowPercentagesOnAllPropertiesOfSpecifiedElementTypes))
			{
				if (!_percentageElementTypesIfEnabled.Any())
					throw new ArgumentException("percentageElementTypesIfEnabled must have at least one value if AllowPercentageWidthsOnSpecifiedElementTypes is enabled");
			}
			else if (_percentageElementTypesIfEnabled.Any())
				throw new ArgumentException("percentageElementTypesIfEnabled may not have any values specified if AllowPercentageWidthsOnSpecifiedElementTypes is not enabled");

			_conformity = conformity;
		}
		public AllMeasurementsMustBePixels(ConformityOptions conformity) : this(conformity, new string[0]) { }

		/// <summary>
		/// If the ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes behaviour is enabled then these are the recommended exceptions: div, td and th
		/// </summary>
		public static IEnumerable<string> RecommendedPercentageExceptions { get { return new[] { "div", "td", "th" }; } }

		public bool DoesThisRuleApplyTo(StyleSheetTypeOptions styleSheetType)
		{
			if (!Enum.IsDefined(typeof(StyleSheetTypeOptions), styleSheetType))
				throw new ArgumentOutOfRangeException("styleSheetType");

			// This can't be applied to compiled stylesheets as the ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypesntageWidthDivs option specifies that img elements
			// are allowed width:100% if the style is nested within a div that has percentage width (when the rules are compiled or combined this nesting will no longer be possible)
			if (styleSheetType == StyleSheetTypeOptions.Compiled)
				return false;

			// Can't apply to combined sheets for the same reason as above
			if (styleSheetType == StyleSheetTypeOptions.Combined)
				return false;

			// There may be some dodgy hacks required in the Reset sheet that uses widths that wouldn't be allowed in other places, so skip processing on thoses
			if (styleSheetType == StyleSheetTypeOptions.Reset)
				return false;

			// For the other types (ie. Themes and Other), we DO want this rule to run
			return true;
		}

		[Flags]
		public enum ConformityOptions
		{
			/// <summary>
			/// This will not allow any element to have any measurement unit that is not in pixels
			/// </summary>
			Strict = 0,

			/// <summary>
			/// This will allow the specified element types to have a width with a percentage unit and img elements whose styles are nested within their style blocks to have
			/// width:100%, the recommended element types for this option are div, td and th - this set is exposed through the static RecommendedPercentageWidthExceptions property
			/// </summary>
			AllowPercentageWidthsOnSpecifiedElementTypes = 1,

			/// <summary>
			/// This will allow any property to be specified as 100% (acceptable for width or font-size, for example)
			/// </summary>
			AllowOneHundredPercentOnAnyElementAndProperty = 2,

			/// <summary>
			/// This will allow the specified element types to have any properties with a percentage unit and img elements whose styles are nested within their style blocks to have
			/// width:100%, the recommended element types for this option are div, td and th - this set is exposed through the static RecommendedPercentageWidthExceptions property.
			/// Note that this takes incorporate the AllowPercentageWidthsOnSpecifiedElementTypes behaviour.
			/// </summary>
			AllowPercentagesOnAllPropertiesOfSpecifiedElementTypes = 5, // 5 = 1 + 4 = AllowOneHundredPercentOnAnyElementAndProperty plus some more relaxing behaviour

			/// <summary>
			/// Animations often use percentages for positioning (as they and will often be sufficiently generic that the precise measurements to use won't be known) and so it may
			/// be desirable to skip all validation within @keyframes declarations
			/// </summary>
			DoNotValidateKeyFramesProperties = 8
		}

		/// <summary>
		/// This will throw an exception if the specified rule BrokenRuleEncounteredException is broken. It will throw an ArgumentException for a null fragments
		/// references, or one which contains a null reference.
		/// </summary>
		public void EnsureRulesAreMet(IEnumerable<ICSSFragment> fragments)
		{
			if (fragments == null)
				throw new ArgumentNullException("fragments");

			var firstBrokenRuleIfAny = GetAnyBrokenRules(fragments).FirstOrDefault();
			if (firstBrokenRuleIfAny != null)
				throw firstBrokenRuleIfAny;
		}

		public IEnumerable<BrokenRuleEncounteredException> GetAnyBrokenRules(IEnumerable<ICSSFragment> fragments)
		{
			IEnumerable<BrokenRuleEncounteredException> brokenRules = new List<BrokenRuleEncounteredException>();

			if (fragments == null)
				throw new ArgumentNullException("Fragments");

			return GetAnyBrokenRules(fragments, new ContainerFragment[0]);
		}

		private IEnumerable<BrokenRuleEncounteredException> GetAnyBrokenRules(IEnumerable<ICSSFragment> fragments, IEnumerable<ContainerFragment> containers)
		{
			if (fragments == null)
				throw new ArgumentNullException("Fragments");
			if (containers == null)
				throw new ArgumentNullException("Fontainers");

			foreach (var fragment in fragments)
			{
				if (fragment == null)
					throw new ArgumentException("Null reference encountered in fragments set");

				var containerFragment = fragment as ContainerFragment;
				if (containerFragment != null)
				{
					foreach (var brokenRule in GetAnyBrokenRules(containerFragment.ChildFragments, containers.Concat(new[] { containerFragment })))
						yield return brokenRule;
					continue;
				}

				var stylePropertyValueFragment = fragment as StylePropertyValue;
				if (stylePropertyValueFragment == null)
					continue;

				if (_conformity.HasFlag(ConformityOptions.DoNotValidateKeyFramesProperties) && containers.OfType<Selector>().Any(s => s.IsKeyFrameDeclaration()))
					continue;

				// Generic tests for measurement units
				var stylePropertyValueFragmentSections = stylePropertyValueFragment.ValueSegments;
				foreach (var value in stylePropertyValueFragmentSections)
				{
					if (_conformity.HasFlag(ConformityOptions.AllowOneHundredPercentOnAnyElementAndProperty) && (value == "100%"))
						continue;

					if (_conformity.HasFlag(ConformityOptions.AllowPercentageWidthsOnSpecifiedElementTypes)
					|| _conformity.HasFlag(ConformityOptions.AllowPercentagesOnAllPropertiesOfSpecifiedElementTypes))
					{
						// To get the parent Selector we have to walk backwards up the containers set since this property value could be wrapped in a MediaQuery - eg.
						// div.Whatever {
						//   @media screen and (max-width:70em) {
						//     width: 50%;
						//   }
						// }
						var directParentSelector = containers.LastOrDefault(c => c is Selector) as Selector;
						if ((directParentSelector != null)
						&& (stylePropertyValueFragment.Property.HasName("width") || _conformity.HasFlag(ConformityOptions.AllowPercentagesOnAllPropertiesOfSpecifiedElementTypes))
						&& IsPercentageMeasurement(value))
						{
							// If the selector for this property targets divs or tds only (eg. "div.Main" or "div.Header div.Logo, div.Footer div.Logo") then allow
							// percentage widths
							if (DoesSelectorTargetOnlyElementsWithTagNames(directParentSelector, _percentageElementTypesIfEnabled))
								continue;

							// If the selector for this property targets imgs only then allow "width:100%" values so long as they are inside a div or td with a percentage width
							if (DoesSelectorTargetOnlyElementsWithTagNames(directParentSelector, new[] { "img" }))
							{
								if (IsPercentageMeasurement(value))
								{
									if (value != "100%")
									{
										yield return new AllMeasurementsMustBePixelsNotAppliedException("The only allow percentage width for img is 100%", fragment);
										continue; // We've already rejected this property value, don't bother looking for other ways in which it may be wrong
									}
									else
									{
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
											.Where(s => s.Property.HasName("width") && s.GetValueSectionsThatAreMeasurements().All(v => v.Unit == "%"))
											.FirstOrDefault();
										if (firstContainerWithPercentageWidth == null)
										{
											yield return new AllMeasurementsMustBePixelsNotAppliedException(
												"Percentage width for img may is only allowable if nested within a div or td style with percentage width (and the img must have width:100%)",
												fragment);
										}
									}
								}

								// This concludes our special-case handling with an img width:100% nested within an allowed %age width container, so stop looking for trouble
								continue;
							}
						}
					}

					// Check for measurements that are a numeric value and a unit string, where the unit string is any other than "px"
					var encounteredInvalidValue = false;
					foreach (var disallowedMeasurementUnit in DisallowedMeasurementUnits)
					{
						if (!value.EndsWith(disallowedMeasurementUnit, StringComparison.InvariantCultureIgnoreCase))
							continue;

						float numericValue;
						if (float.TryParse(value.Substring(0, value.Length - disallowedMeasurementUnit.Length).Trim(), out numericValue))
						{
							yield return new AllMeasurementsMustBePixelsNotAppliedException(fragment);
							encounteredInvalidValue = true;
							break;
						}
					}
					if (!encounteredInvalidValue  && IsPercentageMeasurement(value))
					{
						// If the measurement is a percentage that wasn't caught above then either it's not valid or it uses the "percentage(0.1)" format, either way
						// it's not allowed at this point
						yield return new AllMeasurementsMustBePixelsNotAppliedException(fragment);
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
						yield return new AllMeasurementsMustBePixelsNotAppliedException(fragment);
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
