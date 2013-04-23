using System;
using System.Collections.Generic;
using System.Text;
using CSSParser;
using CSSParser.ExtendedLESSParser;

namespace NonCascadingCSSRulesEnforcer.ExtendedLESSParserExtensions
{
	public static class StylePropertyValue_Extensions
	{
		/// <summary>
		/// Get a property value broken into individual sections - eg. from "3px solid black" extract [ "3px", "solid", "black" ] or from "white url('test.png') top left no-repeat"
		/// extract [ "white",  "url('test.png')", "top", "left", "no-repeat" ]. This will never return null nor a set containing any null or blank values.
		/// </summary>
		public static IEnumerable<string> GetValueSections(this StylePropertyValue source)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			// The CSSParser has to deal with quoting of values so we can use it here - given a property value it should return a set of sections where the only
			// CharacterCategorisation values are Whitespace and Value, any whitespace within a quoted value will be identified as Value, not Whitespace
			var sections = new List<string>();
			var buffer = new StringBuilder();
			foreach (var section in Parser.ParseCSS(source.Value))
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
			foreach (var valueSection in source.GetValueSections())
			{
				if (valueSection == new string('0', valueSection.Length))
				{
					measurementValues.Add(new Measurement(0, null));
					continue;
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
