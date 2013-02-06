using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NonCascadingCSSRulesEnforcer.HierarchicalParsing;

namespace NonCascadingCSSRulesEnforcer.Rules
{
	/// <summary>
	/// This rule only applies to stylesheets other than the Resets and Themes files. No bare selectors may occur, with the exception of child selectors that are nested selectors
	/// </summary>
	public class NoBareSelectorsInNonResetsOrThemeSheets : IEnforceRules
	{
		private readonly ScopeRestrictingBodyTagBehaviourOptions _scopeRestrictingBodyTagBehaviour;
		public NoBareSelectorsInNonResetsOrThemeSheets(ScopeRestrictingBodyTagBehaviourOptions scopeRestrictingBodyTagBehaviour)
		{
			if (!Enum.IsDefined(typeof(ScopeRestrictingBodyTagBehaviourOptions), scopeRestrictingBodyTagBehaviour))
				throw new ArgumentOutOfRangeException("scopeRestrictingBodyTagBehaviour");

			_scopeRestrictingBodyTagBehaviour = scopeRestrictingBodyTagBehaviour;
		}

		public enum ScopeRestrictingBodyTagBehaviourOptions
		{
			Allow,
			Disallow
		}

		/// <summary>
		/// This will throw an exception if the specified rule BrokenRuleEncounteredException is broken. It will throw an ArgumentException for a null fragments
		/// references, or one which contains a null reference.
		/// </summary>
		public void EnsureRulesAreMet(IEnumerable<ICSSFragment> fragments)
		{
			if (fragments == null)
				throw new ArgumentNullException("fragments");

			foreach (var fragment in fragments)
			{
				// If the fragment isn't a selector then we're not interesting in it for this rule
				var selectorFragment = fragment as Selector;
				if (selectorFragment == null)
					continue;

				if (!IsScopeRestrictingBodyTag(selectorFragment) || (_scopeRestrictingBodyTagBehaviour == ScopeRestrictingBodyTagBehaviourOptions.Disallow))
				{
					if (selectorFragment.Selectors.Any(s => !IsValidSelector(s)))
						throw new DisallowBareSelectorsEncounteredException(selectorFragment);
				}

				EnsureRulesAreMet(selectorFragment.ChildFragments);
			}
		}

		/// <summary>
		/// A fragment is identified as a scope-restricting body tag if it is a top-level selector whose only css selectors state "body" (would expect there
		/// to only be a single css selector, but multiple that are all "body" are technically valid). The body tag may only contain other selectors, there
		/// may be no properties that would apply to the body, otherwise the tag is not used solely for scope restrictions. Depending upon the configuration
		/// of this instance, these may be granted exemption from having to meet the IsValidSelector criteria.
		/// </summary>
		private bool IsScopeRestrictingBodyTag(Selector selectorFragment)
		{
			if (selectorFragment == null)
				throw new ArgumentNullException("selectorFragment");

			if (selectorFragment.ParentSelectors.Any() || selectorFragment.Selectors.Any(s => s.Value != "body"))
				return false;

			// The body may not contain any styles, otherwise it isn't solely for scope restriction. If it contains only nested selectors (or mixins) and
			// less values then that's fine. We need to trim out any media query content here, since a body tag that has a nested media query that sets
			// properties will invalidate the body as being for scope-restriction purposes (as the styles will apply to the body tag when the media
			// query criteria are met). The RemoveMediaQueries extension will transform the data as if the media queries were not present in the
			// source content (any styles inside media queries will be lifted up to the level at which the media query appeared).
			var previousFragmentWasLESSValue = false;
			foreach (var childFragment in selectorFragment.ChildFragments.RemoveMediaQueries())
			{
				if ((childFragment is StylePropertyName) && ((StylePropertyName)childFragment).Value.StartsWith("@"));
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

				if (childFragment is Selector)
				{
					previousFragmentWasLESSValue = false;
					continue;
				}

				throw new Exception("Unsupported fragment type encountered: " + childFragment.GetType());
			}
			return true;
		}

		/// <summary>
		/// In order to be a valid selector in this context, there must be no base css selectors unless they are preceded with the child selector symbol
		/// </summary>
		private bool IsValidSelector(Selector.WhiteSpaceNormalisedString cssSelector)
		{
			if (cssSelector == null)
				throw new ArgumentNullException("cssSelector");

			// If it starts with a "@" then we'll assume it's a media query and let it through
			if (cssSelector.Value.StartsWith("@"))
				return true;

			// Do some minor preprocessing to make it easier to break up, align any child selector with the following selector and away from the previous
			// - We'll have to do a single replace-double-space-with-single-space in case we introduced a double space between a selector and a following
			//   child selector symbol (eg. "div.Wrapper > h2" to "div.Wrapper  >h2") but that's the only whitespace concern since the whitespace has
			//   already been normalised (as it's a WhiteSpaceNormalisedString!)
			foreach (var cssSelectorSegment in cssSelector.Value.Replace("> ", ">").Replace(">", " >").Replace("  ", " ").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
			{
				// If the segment has a child selector symbol or contains a class or an id then it's ok, if none of these conditions are met then the
				// selector must be considered invalid (it is a bare selector)
				if (!cssSelectorSegment.StartsWith(">") && !cssSelectorSegment.Contains(".") && !cssSelectorSegment.Contains("#") && !cssSelectorSegment.Contains("@"))
					return false;
			}
			return true;
		}

		public class DisallowBareSelectorsEncounteredException : BrokenRuleEncounteredException
		{
			public DisallowBareSelectorsEncounteredException(Selector selector)
				: base(
					string.Format(
						"Disallow bare selector encountered (\"{0}\" at line {1})",
						GetSelectorForDisplay(selector),
						selector.SourceLineIndex + 1),
					selector
				) { }

			protected DisallowBareSelectorsEncounteredException(SerializationInfo info, StreamingContext context) : base(info, context) { }

			private static string GetSelectorForDisplay(Selector selector)
			{
				if (selector == null)
					throw new ArgumentNullException("selector");

				return string.Join(
					", ",
					selector.Selectors.Select(s => s.Value)
				);
			}
		}
	}
}
