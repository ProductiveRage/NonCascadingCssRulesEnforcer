using System;
using System.Linq;
using CSSParser.ExtendedLESSParser;
using CSSParser.ExtendedLESSParser.ContentSections;

namespace NonCascadingCSSRulesEnforcer.ExtendedLESSParserExtensions
{
	public static class SelectorSet_Extensions
	{
		public static bool OnlyTargetsBareSelectors(this ContainerFragment.SelectorSet source)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			// TODO: Probably need better handling here for stripping out attribute selectors (wrapped in square brackets) before performing
			// this check since there may be string values which cause false positives if there are any instances of "." or "#" in them
			return !source.Any(s => s.Value.Contains(".") || s.Value.Contains("#"));
		}
	}
}
