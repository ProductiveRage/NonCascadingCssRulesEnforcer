using System;
using System.Linq;
using CSSParser.ExtendedLESSParser.ContentSections;

namespace NonCascadingCSSRulesEnforcer.ExtendedLESSParserExtensions
{
	public static class Selector_Extensions
	{
		/// <summary>
		/// A fragment is identified as a scope-restricting html tag if it is a top-level selector whose only css selectors state "html" (would expect there
		/// to only be a single css selector, but multiple that are all "html" are technically valid). The html tag may only contain other selectors, there
		/// may be no properties that would apply to the html, otherwise the tag is not used solely for scope restrictions. Depending upon the configuration
		/// of this instance, these may be granted exemption from having to meet the IsValidSelector criteria.
		/// </summary>
		public static bool IsScopeRestrictingHtmlTag(this Selector source)
		{
			if (source == null)
				throw new ArgumentNullException("selectorFragment");

			if (source.ParentSelectors.Any() || source.Selectors.Any(s => s.Value != "html"))
				return false;

			// The html may not contain any styles, otherwise it isn't solely for scope restriction. If it contains only nested selectors (or mixins) and
			// less values then that's fine. We need to trim out any media query content here, since a html tag that has a nested media query that sets
			// properties will invalidate the html as being for scope-restriction purposes (as the styles will apply to the html tag when the media
			// query criteria are met). The RemoveMediaQueries extension will transform the data as if the media queries were not present in the
			// source content (any styles inside media queries will be lifted up to the level at which the media query appeared).
			var previousFragmentWasLESSValue = false;
			foreach (var childFragment in source.ChildFragments.RemoveMediaQueries())
			{
				if ((childFragment is StylePropertyName) && ((StylePropertyName)childFragment).Value.StartsWith("@"))
				{
					previousFragmentWasLESSValue = true;
					break;
				}

				if (childFragment is StylePropertyName)
				{
					if (!previousFragmentWasLESSValue)
						return false;

					previousFragmentWasLESSValue = false;
					continue;
				}

				if ((childFragment is Selector) || (childFragment is MediaQuery))
				{
					previousFragmentWasLESSValue = false;
					continue;
				}

				throw new Exception("Unsupported fragment type encountered: " + childFragment.GetType());
			}
			return true;
		}

		public static bool IsBareSelector(this Selector source)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			return source.Selectors.OnlyTargetsBareSelectors();
		}
	}
}
