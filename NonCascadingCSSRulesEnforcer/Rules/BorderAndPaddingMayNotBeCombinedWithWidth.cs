using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CSSParser.ExtendedLESSParser.ContentSections;
using NonCascadingCSSRulesEnforcer.ExtendedLESSParserExtensions;

namespace NonCascadingCSSRulesEnforcer.Rules
{
	/// <summary>
	/// No style block may specify a width with a non-zero padding or border width
	/// </summary>
	public class BorderAndPaddingMayNotBeCombinedWithWidth : IEnforceRules
	{
		private readonly ConformityOptions _conformity;
		public BorderAndPaddingMayNotBeCombinedWithWidth(ConformityOptions conformity)
		{
			if ((_conformity != ConformityOptions.Strict)
			&& (_conformity != ConformityOptions.AllowVerticalBorderAndPadding)
			&& (_conformity != ConformityOptions.IgnoreRuleIfBorderBoxSizingRulePresent)
			&& (_conformity != (ConformityOptions.AllowVerticalBorderAndPadding | ConformityOptions.IgnoreRuleIfBorderBoxSizingRulePresent)))
				throw new ArgumentOutOfRangeException("conformity", "invalid value or combination of values");

			_conformity = conformity;
		}

		[Flags]
		public enum ConformityOptions
		{
			Strict = 0,
			AllowVerticalBorderAndPadding = 1,
			IgnoreRuleIfBorderBoxSizingRulePresent = 2
		}

		public bool DoesThisRuleApplyTo(StyleSheetTypeOptions styleSheetType)
		{
			if (!Enum.IsDefined(typeof(StyleSheetTypeOptions), styleSheetType))
				throw new ArgumentOutOfRangeException("styleSheetType");

			return true;
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
			if (fragments == null)
				throw new ArgumentNullException("fragments");

			foreach (var fragment in fragments)
			{
				var containerFragment = fragment as ContainerFragment;
				if (containerFragment == null)
					continue;
				
				// Try to determine whether a width has been explicitly set
				// - If there are multiple then the last one will be applied by the browser, so consider that (if any have "!important" specified then they
				//   may override, so the styles are ordered so that those without "!important" appear first and then those that do, it is the last entry
				//   of this set that we want)
				var propertyValues = containerFragment.ChildFragments
					.Select((f, i) => new { Fragment = f, Index = i })
					.Where(f => f.Fragment is StylePropertyValue)
					.Select(f => new { PropertyValueFragment = (StylePropertyValue)f.Fragment, Index = f.Index })
					.OrderBy(f => f.PropertyValueFragment.ValueSegments.Any(v => v.Equals("!important", StringComparison.InvariantCultureIgnoreCase)) ? 1 : 0)
					.ThenBy(f => f.Index)
					.Select(f => f.PropertyValueFragment);
				var lastWidthPropertyIfAny = propertyValues.LastOrDefault(p => p.Property.Value.Equals("width", StringComparison.InvariantCultureIgnoreCase));
				var isWidthDefined = (
					(lastWidthPropertyIfAny != null) &&
					!!lastWidthPropertyIfAny.GetValueSectionsThatAreMeasurements().Any()
				);

				// If a width has been explicitly set then ensure that no properties are present that violate this rule (2014-08-28: Unless "border-box"
				// has been set for the "box-sizing" property and the confirmity setting has been enabled which allows this). Requiring the "border-box"
				// style to appear within any block that requires it means it's very easy to tell at a glance whether the block requires a modern browser
				// (in this case, that means *not* IE7, since IE7 doesn't support "box-sizing: border-box").
				if (isWidthDefined)
				{
					var lastBorderBoxPropertyIfAny = propertyValues.LastOrDefault(p => p.Property.Value.Equals("box-sizing", StringComparison.InvariantCultureIgnoreCase));
					bool isBorderBoxSizingSpecified;
					if (lastBorderBoxPropertyIfAny == null)
						isBorderBoxSizingSpecified = false;
					else
					{
						var concatenatedPropertyValue = string.Join(", ", lastBorderBoxPropertyIfAny.ValueSegments);
						isBorderBoxSizingSpecified =
							concatenatedPropertyValue.Equals("border-box", StringComparison.InvariantCultureIgnoreCase) ||
							concatenatedPropertyValue.Equals("border-box !important", StringComparison.InvariantCultureIgnoreCase);
					}
					if (!isBorderBoxSizingSpecified || ((_conformity & ConformityOptions.IgnoreRuleIfBorderBoxSizingRulePresent) == 0))
					{
						var paddingDimensions = new SpecifiedDimensionSummary(null, null, null, null);
						var borderDimensions = new SpecifiedDimensionSummary(null, null, null, null);
						{
							foreach (var property in propertyValues)
							{
								paddingDimensions = ProcessAnyPaddingChanges(property, paddingDimensions);
								borderDimensions = ProcessAnyBorderChanges(property, paddingDimensions);
							}
						}

						// Check whether any property has been identified as setting a border or padding dimension
						// - Unless ConformityOptions.Strict behaviour is enabled, ignore the vertial (Top/Bottom) values, the horizontal
						//   ones are of greater importance in relation to width
						var invalidProperty =
							((_conformity == ConformityOptions.Strict) ? paddingDimensions.PropertyWithNonZeroTopValueIfAny : null) ??
							paddingDimensions.PropertyWithNonZeroRightValueIfAny ??
							((_conformity == ConformityOptions.Strict) ? paddingDimensions.PropertyWithNonZeroBottomValueIfAny : null) ??
							paddingDimensions.PropertyWithNonZeroLeftValueIfAny ??
							((_conformity == ConformityOptions.Strict) ? borderDimensions.PropertyWithNonZeroTopValueIfAny : null) ??
							borderDimensions.PropertyWithNonZeroRightValueIfAny ??
							((_conformity == ConformityOptions.Strict) ? borderDimensions.PropertyWithNonZeroBottomValueIfAny : null) ??
							borderDimensions.PropertyWithNonZeroLeftValueIfAny;
                        if (invalidProperty != null)
                            yield return new BorderAndPaddingMayNotBeCombinedWithWidthException(invalidProperty);
					}
				}

                foreach (var brokenRule in GetAnyBrokenRules(containerFragment.ChildFragments))
                    yield return brokenRule;
			}
		}

		private SpecifiedDimensionSummary ProcessAnyPaddingChanges(StylePropertyValue property, SpecifiedDimensionSummary dimensionsSummary)
		{
			if (property == null)
				throw new ArgumentNullException("property");
			if (dimensionsSummary == null)
				throw new ArgumentNullException("dimensionsSummary");

			var measurements = property.GetValueSectionsThatAreMeasurements().ToArray();
			if (!measurements.Any())
				return dimensionsSummary;

			if (property.Property.Value.Equals("padding-top", StringComparison.InvariantCultureIgnoreCase))
			{
				return new SpecifiedDimensionSummary(
					(measurements.First().Value == 0) ? null : property,
					dimensionsSummary.PropertyWithNonZeroRightValueIfAny,
					dimensionsSummary.PropertyWithNonZeroBottomValueIfAny,
					dimensionsSummary.PropertyWithNonZeroLeftValueIfAny
				);
			}
			else if (property.Property.Value.Equals("padding-right", StringComparison.InvariantCultureIgnoreCase))
			{
				return new SpecifiedDimensionSummary(
					dimensionsSummary.PropertyWithNonZeroTopValueIfAny,
					(measurements.First().Value == 0) ? null : property,
					dimensionsSummary.PropertyWithNonZeroBottomValueIfAny,
					dimensionsSummary.PropertyWithNonZeroLeftValueIfAny
				);
			}
			else if (property.Property.Value.Equals("padding-bottom", StringComparison.InvariantCultureIgnoreCase))
			{
				return new SpecifiedDimensionSummary(
					dimensionsSummary.PropertyWithNonZeroTopValueIfAny,
					dimensionsSummary.PropertyWithNonZeroRightValueIfAny,
					(measurements.First().Value == 0) ? null : property,
					dimensionsSummary.PropertyWithNonZeroLeftValueIfAny
				);
			}
			else if (property.Property.Value.Equals("padding-left", StringComparison.InvariantCultureIgnoreCase))
			{
				return new SpecifiedDimensionSummary(
					dimensionsSummary.PropertyWithNonZeroTopValueIfAny,
					dimensionsSummary.PropertyWithNonZeroRightValueIfAny,
					dimensionsSummary.PropertyWithNonZeroBottomValueIfAny,
					(measurements.First().Value == 0) ? null : property
				);
			}
			else if (!property.Property.Value.Equals("padding", StringComparison.InvariantCultureIgnoreCase))
				return dimensionsSummary;

			// The "padding" property may have one or four dimensions specified
			//  - If only one then all sides have that value
			//  - If two then the settings are for top/bottom and left/right
			//  - If three then the settingss are for top, left/right, bottom
			//  - If four then the settings are for top, right, bottom, left
			var topValue = measurements[0].Value;
			var rightValue = (measurements.Length > 1) ? measurements[1].Value : topValue;
			var bottomValue = (measurements.Length > 2) ? measurements[2].Value : topValue;
			var leftValue = (measurements.Length > 3) ? measurements[3].Value : rightValue;
			return new SpecifiedDimensionSummary(
				(topValue == 0) ? null : property,
				(rightValue == 0) ? null : property,
				(bottomValue == 0) ? null : property,
				(leftValue == 0) ? null : property
			);
		}

		private SpecifiedDimensionSummary ProcessAnyBorderChanges(StylePropertyValue property, SpecifiedDimensionSummary dimensionsSummary)
		{
			if (property == null)
				throw new ArgumentNullException("property");
			if (dimensionsSummary == null)
				throw new ArgumentNullException("dimensionsSummary");

			// Address the "border:none" case, this is the only property / value combination that can affect width without specifying dimensions
			if (property.Property.Value.Equals("border", StringComparison.InvariantCultureIgnoreCase)
			&& property.ValueSegments.Any(v => v.Equals("none", StringComparison.InvariantCultureIgnoreCase)))
				return new SpecifiedDimensionSummary(null, null, null, null);

			// Border widths can be specified with {value}{unit} or by a predefined width string (eg. "thin"), to take this complication out of the
			// picture any pre-defined width strings are replaced with "4px" before trying to extract measurements values from the property's value
			var propertyWithPreDefinedWidthReplaced = new StylePropertyValue(
				property.Property,
				property.ValueSegments.Select(s => ValueIsPreDefinedWidthValue(s) ? "4px" : s),
				property.SourceLineIndex
			);
			var measurements = propertyWithPreDefinedWidthReplaced.GetValueSectionsThatAreMeasurements().ToArray();
			if (!measurements.Any())
				return dimensionsSummary;

			if (property.Property.Value.Equals("border-top", StringComparison.InvariantCultureIgnoreCase)
			|| property.Property.Value.Equals("border-top-width", StringComparison.InvariantCultureIgnoreCase))
			{
				return new SpecifiedDimensionSummary(
					(measurements.First().Value == 0) ? null : property,
					dimensionsSummary.PropertyWithNonZeroRightValueIfAny,
					dimensionsSummary.PropertyWithNonZeroBottomValueIfAny,
					dimensionsSummary.PropertyWithNonZeroLeftValueIfAny
				);
			}
			else if (property.Property.Value.Equals("border-right", StringComparison.InvariantCultureIgnoreCase)
			|| property.Property.Value.Equals("border-right-width", StringComparison.InvariantCultureIgnoreCase))
			{
				return new SpecifiedDimensionSummary(
					dimensionsSummary.PropertyWithNonZeroTopValueIfAny,
					(measurements.First().Value == 0) ? null : property,
					dimensionsSummary.PropertyWithNonZeroBottomValueIfAny,
					dimensionsSummary.PropertyWithNonZeroLeftValueIfAny
				);
			}
			else if (property.Property.Value.Equals("border-bottom", StringComparison.InvariantCultureIgnoreCase)
			|| property.Property.Value.Equals("border-bottom-width", StringComparison.InvariantCultureIgnoreCase))
			{
				return new SpecifiedDimensionSummary(
					dimensionsSummary.PropertyWithNonZeroTopValueIfAny,
					dimensionsSummary.PropertyWithNonZeroRightValueIfAny,
					(measurements.First().Value == 0) ? null : property,
					dimensionsSummary.PropertyWithNonZeroLeftValueIfAny
				);
			}
			else if (property.Property.Value.Equals("border-left", StringComparison.InvariantCultureIgnoreCase)
			|| property.Property.Value.Equals("border-left-width", StringComparison.InvariantCultureIgnoreCase))
			{
				return new SpecifiedDimensionSummary(
					dimensionsSummary.PropertyWithNonZeroTopValueIfAny,
					dimensionsSummary.PropertyWithNonZeroRightValueIfAny,
					dimensionsSummary.PropertyWithNonZeroBottomValueIfAny,
					(measurements.First().Value == 0) ? null : property
				);
			}
			else if (property.Property.Value.Equals("border", StringComparison.InvariantCultureIgnoreCase))
			{
				// The "border" property can only specify a single dimension which applies to all sides (unlike "border-width", see below)
				return new SpecifiedDimensionSummary(
					(measurements.First().Value == 0) ? null : property,
					(measurements.First().Value == 0) ? null : property,
					(measurements.First().Value == 0) ? null : property,
					(measurements.First().Value == 0) ? null : property
				);
			}
			else if (!property.Property.Value.Equals("border-width", StringComparison.InvariantCultureIgnoreCase))
				return dimensionsSummary;

			// The "border-width" property is like "padding" and may have between and and four dimensions
			var topValue = measurements[0].Value;
			var rightValue = (measurements.Length > 1) ? measurements[1].Value : topValue;
			var bottomValue = (measurements.Length > 2) ? measurements[2].Value : topValue;
			var leftValue = (measurements.Length > 3) ? measurements[3].Value : rightValue;
			return new SpecifiedDimensionSummary(
				(topValue == 0) ? null : property,
				(rightValue == 0) ? null : property,
				(bottomValue == 0) ? null : property,
				(leftValue == 0) ? null : property
			);
		}

		private bool ValueIsPreDefinedWidthValue(string value)
		{
			if (value == null)
				throw new ArgumentNullException("value");

			return
				value.Equals("thin", StringComparison.InvariantCultureIgnoreCase) ||
				value.Equals("medium", StringComparison.InvariantCultureIgnoreCase) ||
				value.Equals("thick", StringComparison.InvariantCultureIgnoreCase);
		}

		private class SpecifiedDimensionSummary
		{
			public SpecifiedDimensionSummary(
				StylePropertyValue propertyWithNonZeroTopValueIfAny,
				StylePropertyValue propertyWithNonZeroRightValueIfAny,
				StylePropertyValue propertyWithNonZeroBottomValueIfAny,
				StylePropertyValue propertyWithNonZeroLeftValueIfAny)
			{
				PropertyWithNonZeroTopValueIfAny = propertyWithNonZeroTopValueIfAny;
				PropertyWithNonZeroRightValueIfAny = propertyWithNonZeroRightValueIfAny;
				PropertyWithNonZeroBottomValueIfAny = propertyWithNonZeroBottomValueIfAny;
				PropertyWithNonZeroLeftValueIfAny = propertyWithNonZeroLeftValueIfAny;
			}
			public StylePropertyValue PropertyWithNonZeroTopValueIfAny { get; private set; }
			public StylePropertyValue PropertyWithNonZeroRightValueIfAny { get; private set; }
			public StylePropertyValue PropertyWithNonZeroBottomValueIfAny { get; private set; }
			public StylePropertyValue PropertyWithNonZeroLeftValueIfAny { get; private set; }
		}

		public class BorderAndPaddingMayNotBeCombinedWithWidthException : BrokenRuleEncounteredException
		{
			public BorderAndPaddingMayNotBeCombinedWithWidthException(ICSSFragment fragment) : base("Style block encountered that combines border and/or padding with width", fragment) { }
			protected BorderAndPaddingMayNotBeCombinedWithWidthException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		}
	}
}
