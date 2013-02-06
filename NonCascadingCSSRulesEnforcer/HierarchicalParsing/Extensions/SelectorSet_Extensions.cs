using System;
using System.Linq;

namespace NonCascadingCSSRulesEnforcer.HierarchicalParsing
{
	public static class SelectorSet_Extensions
	{
		public static bool IndicatesMediaQuery(this Selector.SelectorSet source)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			return source.First().Value.StartsWith("@media");
		}
	}
}
