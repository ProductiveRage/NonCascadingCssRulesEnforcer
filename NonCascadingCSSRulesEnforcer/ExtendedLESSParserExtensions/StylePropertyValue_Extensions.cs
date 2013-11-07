using System;
using System.Collections.Generic;
using CSSParser.ExtendedLESSParser.ContentSections;

namespace NonCascadingCSSRulesEnforcer.ExtendedLESSParserExtensions
{
	public static class StylePropertyValue_Extensions
	{
		/// <summary>
		/// This performs a case-insensitive match against the combined ValueSegments set of a property value (the segments are combined by joining around
		/// single space characters)
		/// </summary>
		public static bool HasValue(this StylePropertyValue source, string value)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			if (value == null)
				throw new ArgumentNullException("value");

			return string.Join(" ", source.ValueSegments).Equals(value, StringComparison.InvariantCultureIgnoreCase);
		}

		/// <summary>
		/// Extract any measurements from a property value - eg. "3px solid black" will return a set containing a Measurement with Value 3 and Unit "px", a
		/// value of "0" will return a set containing a Measurement with Value 0 and null Unit, a value of "2px 2px" will return a set containing two
		/// Measurement instances, both with Value 2 and Unit "px"
		/// </summary>
		public static IEnumerable<Measurement> GetValueSectionsThatAreMeasurements(this StylePropertyValue source)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			var measurementValues = new List<Measurement>();
			foreach (var valueSection in source.ValueSegments)
			{
				if (valueSection == new string('0', valueSection.Length))
				{
					measurementValues.Add(new Measurement(0, null));
					continue;
				}

				// 2013-11-07 DWR: This wasn't previously identifying "percentage(0.1)", which is equivalent to "10%"
				var percentageLabel = "percentage";
				if (valueSection.StartsWith(percentageLabel + "(", StringComparison.InvariantCultureIgnoreCase) && valueSection.EndsWith(")"))
				{
					float numericValue;
					if (float.TryParse(valueSection.Substring(percentageLabel.Length + 1, valueSection.Length - (percentageLabel.Length + 2)).Trim(), out numericValue))
						measurementValues.Add(new Measurement(numericValue * 100, "%"));
				}

				foreach (var measurementUnit in Constants.MeasurementUnits)
				{
					if (!valueSection.EndsWith(measurementUnit, StringComparison.InvariantCultureIgnoreCase))
						continue;

					float numericValue;
					if (float.TryParse(valueSection.Substring(0, valueSection.Length - measurementUnit.Length).Trim(), out numericValue))
						measurementValues.Add(new Measurement(numericValue, measurementUnit));
				}
			}
			return measurementValues;
		}

		public class Measurement
		{
			public Measurement(float value, string unit)
			{
				if ((value != 0) && (unit == null))
					throw new ArgumentException("unit may only be null if value is zero");

				Value = value;
				Unit = unit;
			}
			
			public float Value { get; private set; }

			/// <summary>
			/// This may or may not be null if Value is zero but will not be null otherwise
			/// </summary>
			public string Unit { get; private set; }
		}
	}
}
