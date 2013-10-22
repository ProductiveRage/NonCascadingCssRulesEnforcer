using System;
using CSSParser.ExtendedLESSParser.ContentSections;

namespace NonCascadingCSSRulesEnforcer.ExtendedLESSParserExtensions
{
	public static class StylePropertyName_Extensions
	{
		/// <summary>
		/// This will perform a case-insensitive match against the property name
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
