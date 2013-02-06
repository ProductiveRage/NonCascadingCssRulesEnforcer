using System;
using System.Linq;

namespace NonCascadingCSSRulesEnforcer.HierarchicalParsing
{
	public static class Selector_Extensions
	{
		public static bool IsMediaQuery(this Selector source)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			return source.Selectors.IndicatesMediaQuery();
		}
	}
}
