using System;
using System.Collections.Generic;
using System.Linq;
using CSSParser.ExtendedLESSParser.ContentSections;

namespace NonCascadingCSSRulesEnforcer.ExtendedLESSParserExtensions
{
	public static class IEnumerableICSSFragment_Extensions
	{
		/// <summary>
		/// This will remove any media query selectors from the set, applied recursively through all ChildFragments of any Selector instances. Any record of the media
		/// queries will be removed from remaining Selectors' ParentSelectors set, where present. It will be as if the media queries were never present in the source
		/// and any wrapped content is promoted to the the level at which the media query was found.
		/// </summary>
		public static IEnumerable<ICSSFragment> RemoveMediaQueries(this IEnumerable<ICSSFragment> fragments)
		{
			if (fragments == null)
				throw new ArgumentNullException("fragments");

			var fragmentsWithoutMediaQueries = new List<ICSSFragment>();
			foreach (var fragment in fragments)
			{
				if (fragment == null)
					throw new ArgumentException("Encountered null reference in fragments set");

				var selectorFragment = fragment as Selector;
				if (selectorFragment != null)
				{
					fragmentsWithoutMediaQueries.Add(
						new Selector(
							selectorFragment.Selectors,
							selectorFragment.ParentSelectors.Where(s => !s.First().Value.StartsWith("@media", StringComparison.InvariantCultureIgnoreCase)),
							selectorFragment.SourceLineIndex,
							RemoveMediaQueries(selectorFragment.ChildFragments)
						)
					);
					continue;
				}

				var mediaQueryFragment = fragment as MediaQuery;
				if (mediaQueryFragment != null)
				{
					fragmentsWithoutMediaQueries.AddRange(
						RemoveMediaQueries(mediaQueryFragment.ChildFragments)
					);
					continue;
				}

				fragmentsWithoutMediaQueries.Add(fragment);
			}
			return fragmentsWithoutMediaQueries;
		}
	}
}
