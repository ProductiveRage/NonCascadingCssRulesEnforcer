using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using CSSParser;
using CSSParser.ExtendedLESSParser;

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
			if (!Enum.IsDefined(typeof(ConformityOptions), conformity))
				throw new ArgumentOutOfRangeException("conformity");

			_conformity = conformity;
		}

		public bool DoesThisRuleApplyTo(StyleSheetTypeOptions styleSheetType)
		{
			if (!Enum.IsDefined(typeof(StyleSheetTypeOptions), styleSheetType))
				throw new ArgumentOutOfRangeException("styleSheetType");

			// This can't be applied to compiled stylesheets as the ConformityOptions.AllowPercentageWidthDivs option specifies that img elements are allowed width:100% if
			// the style is nested within a div that has percentage width (when the rules are compiled this nesting will no longer be possible)
			return styleSheetType != StyleSheetTypeOptions.Compiled;
		}

		public enum ConformityOptions
		{
			/// <summary>
			/// This will allow div elements to have a width with a percentage unit and img elements whose styles are nested within the div style block to have width:100%
			/// </summary>
			AllowPercentageWidthDivs,

			/// <summary>
			/// This will not allow any element to have any measurement unit that is not in pixels
			/// </summary>
			Strict
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

			StylePropertyName stylePropertyName = null;
			foreach (var fragment in fragments)
			{
				if (fragment == null)
					throw new ArgumentException("Null reference encountered in fragments set");

				var containerFragmentFragment = fragment as ContainerFragment;
				if (containerFragmentFragment != null)
				{
					EnsureRulesAreMet(containerFragmentFragment.ChildFragments, containers.Concat(new[] { containerFragmentFragment }));
					continue;
				}

				if (fragment is StylePropertyName)
					stylePropertyName = (StylePropertyName)fragment;

				var stylePropertyValueFragment = fragment as StylePropertyValue;
				if (stylePropertyValueFragment == null)
					continue;

				// Generic tests for measurement units
				var stylePropertyValueFragmentSections = GetPropertyValueSections(stylePropertyValueFragment.Value);
				foreach (var value in stylePropertyValueFragmentSections)
				{
					if (_conformity == ConformityOptions.AllowPercentageWidthDivs)
					{
						// If ConformityOptions.AllowPercentageWidthDivs was specified then allow div elements to have a percentage width and for img elements within those
						// divs (the style must be declared nested within the div style as supported by LESS, it is not sufficient
						// - See http://www.productiverage.com/Read/46 for justification for allowing the relaxation of this rule
						// - The HTML5 "section" element should have semantic meaning, "div" is still appropriate as a container if no semantic meaning is attached, as such
						//   it is acceptable that only div wrappers may have "width:100%" (see http://webdesign.about.com/od/html5tags/a/when-to-use-section-element.htm
						//   and http://html5doctor.com/you-can-still-use-div)
						var directParentSelector = containers.LastOrDefault() as Selector;
						if ((directParentSelector != null)
						&& stylePropertyValueFragment.Property.Value.Equals("width", StringComparison.InvariantCultureIgnoreCase)
						&& value.EndsWith("%", StringComparison.InvariantCultureIgnoreCase))
						{
							// If the selector for this property targets divs only (eg. "div.Main" or "div.Header div.Logo, div.Footer div.Logo") then allow percentage widths
							if (DoesSelectorTargetOnlyElementsWithTagName(directParentSelector, "div"))
								continue;

							// If the selector for this property targets imgs only then allow "width:100%" values so long as they are inside a div with a percentage width
							if (DoesSelectorTargetOnlyElementsWithTagName(directParentSelector, "img"))
							{
								if (value.EndsWith("%"))
								{
									if (value != "100%")
										throw new AllMeasurementsMustBePixelsNotAppliedException("The only allow percentage width for img is 100%", fragment);

									if (containers
										.Where(s => (s is Selector) && DoesSelectorTargetOnlyElementsWithTagName((Selector)s, "div"))
										.SelectMany(s => s.ChildFragments)
										.Where(f => f is StylePropertyValue)
										.Cast<StylePropertyValue>()
										.Where(s => s.Property.Value.Equals("width", StringComparison.InvariantCultureIgnoreCase) && s.Value.EndsWith("%"))
										.Any())
										continue;

									throw new AllMeasurementsMustBePixelsNotAppliedException("Percentage width for img may is only allowable if nested within a div style with percentage width (and the img must have width:100%)", fragment);
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
				if (stylePropertyName != null)
				{
					// Border widths must be explicitly specified, use of "thin", "medium" or "thick" are not allowed
					var stylePropertyNameValue = stylePropertyName.Value.ToLower();
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
		}

		/// <summary>
		/// Taken from http://www.w3.org/TR/css3-values/#font-relative-lengths (all values except "px")
		/// </summary>
		private static readonly string[] DisallowedMeasurementUnits = new[] { "em", "ex", "ch", "rem", "vw", "vh", "vmin", "vmax", "cm", "mm", "in", "pt", "pc", "%" };

		/// <summary>
		/// Break a property value into sections - eg. "3px solid black" into [ "3px", "solid", "black" ] or "white url('test.png') top left no-repeat" into [ "white",
		/// "url('test.png')", "top", "left", "no-repeat" ]. This will never return null nor a set containing any null or blank values.
		/// </summary>
		private IEnumerable<string> GetPropertyValueSections(string value)
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

		private bool DoesSelectorTargetOnlyElementsWithTagName(Selector parentSelector, string tagName)
		{
			if (parentSelector == null)
				throw new ArgumentNullException("parentSelector");
			if (string.IsNullOrWhiteSpace(tagName))
				throw new ArgumentException("Null/blank tagName specified");

			tagName = tagName.Trim();
			foreach (var finalSelectorSegment in parentSelector.Selectors.Select(s => s.Value.Split(' ').Last()))
			{
				var targetedTagName = finalSelectorSegment.Split(new[] { '.', '#', ':' }).First();
				if (!targetedTagName.Equals(tagName, StringComparison.InvariantCultureIgnoreCase))
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
