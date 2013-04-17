using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NonCascadingCSSRulesEnforcer.HierarchicalParsing;

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

			EnsureRulesAreMet(fragments, new Selector[0]);
		}

		private void EnsureRulesAreMet(IEnumerable<ICSSFragment> fragments, IEnumerable<Selector> parentSelectors)
		{
			if (fragments == null)
				throw new ArgumentNullException("fragments");
			if (parentSelectors == null)
				throw new ArgumentNullException("parentSelectors");

			StylePropertyName stylePropertyName = null;
			foreach (var fragment in fragments)
			{
				if (fragment == null)
					throw new ArgumentException("Null reference encountered in fragments set");

				var selectorFragment = fragment as Selector;
				if (selectorFragment != null)
				{
					EnsureRulesAreMet(selectorFragment.ChildFragments, parentSelectors.Concat(new[] { selectorFragment }));
					continue;
				}

				if (fragment is StylePropertyName)
					stylePropertyName = (StylePropertyName)fragment;

				var stylePropertyValueFragment = fragment as StylePropertyValue;
				if (stylePropertyValueFragment == null)
					continue;

				var value = stylePropertyValueFragment.Value;
				if (_conformity == ConformityOptions.AllowPercentageWidthDivs)
				{
					// If ConformityOptions.AllowPercentageWidthDivs was specified then allow div elements to have a percentage width and for img elements within those
					// divs (the style must be declared nested within the div style as supported by LESS, it is not sufficient
					// - See http://www.productiverage.com/Read/46 for justification for allowing the relaxation of this rule
					// - The HTML5 "section" element should have semantic meaning, "div" is still appropriate as a container if no semantic meaning is attached, as such
					//   it is acceptable that only div wrappers may have "width:100%" (see http://webdesign.about.com/od/html5tags/a/when-to-use-section-element.htm
					//   and http://html5doctor.com/you-can-still-use-div)
					var directParent = parentSelectors.LastOrDefault();
					if ((directParent != null)
					&& stylePropertyValueFragment.Property.Value.Equals("width", StringComparison.InvariantCultureIgnoreCase)
					&& value.EndsWith("%", StringComparison.InvariantCultureIgnoreCase))
					{
						// If the selector for this property targets divs only (eg. "div.Main" or "div.Header div.Logo, div.Footer div.Logo") then allow percentage widths
						if (DoesSelectorTargetOnlyElementsWithTagName(directParent, "div"))
							continue;

						// If the selector for this property targets imgs only then allow "width:100%" values so long as they are inside a div with a percentage width
						if (DoesSelectorTargetOnlyElementsWithTagName(directParent, "img"))
						{
							if (value.EndsWith("%"))
							{
								if (value != "100%")
									throw new AllMeasurementsMustBePixelsNotAppliedException("The only allow percentage width for img is 100%", fragment);

								if (parentSelectors
									.Where(s => DoesSelectorTargetOnlyElementsWithTagName(s, "div"))
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
		}

		/// <summary>
		/// Taken from http://www.w3.org/TR/css3-values/#font-relative-lengths (all values except "px")
		/// </summary>
		private static readonly string[] DisallowedMeasurementUnits = new[] { "em", "ex", "ch", "rem", "vw", "vh", "vmin", "vmax", "cm", "mm", "in", "pt", "pc", "%" };

		private bool DoesSelectorTargetOnlyElementsWithTagName(Selector selector, string tagName)
		{
			if (selector == null)
				throw new ArgumentNullException("selector");
			if (string.IsNullOrWhiteSpace(tagName))
				throw new ArgumentException("Null/blank tagName specified");

			tagName = tagName.Trim();
			foreach (var finalSelectorSegment in selector.Selectors.Select(s => s.Value.Split(' ').Last()))
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
