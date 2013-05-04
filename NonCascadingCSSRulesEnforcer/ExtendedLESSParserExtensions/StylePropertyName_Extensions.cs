using System;
using CSSParser.ExtendedLESSParser;

namespace NonCascadingCSSRulesEnforcer.ExtendedLESSParserExtensions
{
	public static class StylePropertyName_Extensions
	{
		/// <summary>
		/// TODO
		/// </summary>
		public static bool HasName(this StylePropertyName source, string value)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			if (value == null)
				throw new ArgumentNullException("value");

			return source.Value.Equals(value, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}
