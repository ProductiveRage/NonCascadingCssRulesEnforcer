using System;
using System.Linq;

namespace NonCascadingCSSRulesEnforcer.HierarchicalParsing
{
	public class WhiteSpaceNormalisedString
	{
		public WhiteSpaceNormalisedString(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				throw new ArgumentException("Null/blank value specified");

			var valueTidied = new string(value.Select(c => char.IsWhiteSpace(c) ? ' ' : c).ToArray());
			while (valueTidied.Contains("  "))
				valueTidied = valueTidied.Replace("  ", " ");
			valueTidied = valueTidied.Trim();

			Value = valueTidied;
		}

		/// <summary>
		/// All whitespace characters will be replaced with spaces, than any runs of spaces will be replaced with single instances, finally
		/// the string will be trimmed. This will always have some content and never be null or blank.
		/// </summary>
		public string Value { get; private set; }
	}
}
