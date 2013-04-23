using System.Collections.Generic;

namespace NonCascadingCSSRulesEnforcer
{
	public static class Constants
	{
		/// <summary>
		/// Taken from http://www.w3.org/TR/css3-values/#font-relative-lengths
		/// </summary>
		public static IEnumerable<string> MeasurementUnits
		{
			get { return new[] { "em", "ex", "ch", "rem", "vw", "vh", "vmin", "vmax", "cm", "mm", "in", "pt", "pc", "px", "%" }; }
		}
	}
}
