using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CSSParser.ExtendedLESSParser.ContentSections;
using NonCascadingCSSRulesEnforcer.ExtendedLESSParserExtensions;

namespace NonCascadingCSSRulesEnforcer.Rules
{
	/// <summary>
	/// This rule only applies to stylesheets other than the Resets and Themes files. No bare selectors may occur, with the exception of child selectors that are nested selectors
	/// </summary>
	public class NoBareSelectorsInNonResetsOrThemeSheets : IEnforceRules
	{
		/// <summary>
		/// The recommended configuration for this rule is to allow a bare html tag to be use in non-Resets-or-Themes style sheets (this rule doesn't apply to Resets or Themes
		/// sheets) so long as the html tag is the outer most tag and does not directly contain any rules - in other words, if it a scope-restricting-html-tag as tested for by
		/// the HtmlTagScopingMustBeAppliedToNonResetsOrThemesSheets rule. If the recommendation to use a scope-restricting-html-tag is not part of the guidelines being tested
		/// for then a NoBareSelectorsInNonResetsOrThemeSheets instance with ScopeRestrictingHtmlTagBehaviourOptions.Disallow should be used.
		/// </summary>
		public static NoBareSelectorsInNonResetsOrThemeSheets Recommended => _recommended;
		private static NoBareSelectorsInNonResetsOrThemeSheets _recommended = new NoBareSelectorsInNonResetsOrThemeSheets(ScopeRestrictingHtmlTagBehaviourOptions.Allow);

		private readonly ScopeRestrictingHtmlTagBehaviourOptions _scopeRestrictingHtmlTagBehaviour;
		public NoBareSelectorsInNonResetsOrThemeSheets(ScopeRestrictingHtmlTagBehaviourOptions scopeRestrictingHtmlTagBehaviour)
		{
			if (!Enum.IsDefined(typeof(ScopeRestrictingHtmlTagBehaviourOptions), scopeRestrictingHtmlTagBehaviour))
				throw new ArgumentOutOfRangeException("scopeRestrictingHtmlTagBehaviour");

			_scopeRestrictingHtmlTagBehaviour = scopeRestrictingHtmlTagBehaviour;
		}

		public enum ScopeRestrictingHtmlTagBehaviourOptions
		{
			Allow,
			Disallow
		}

		public bool DoesThisRuleApplyTo(StyleSheetTypeOptions styleSheetType)
		{
			if (!Enum.IsDefined(typeof(StyleSheetTypeOptions), styleSheetType))
				throw new ArgumentOutOfRangeException("styleSheetType");

			// This can only apply to "Other" stylesheets since anything else may be Resets or Themes or contain Resets or Themes content (ie. Combined stylesheet content)
			return (styleSheetType == StyleSheetTypeOptions.Other);
		}

		/// <summary>
		/// This will throw an exception if the specified rule BrokenRuleEncounteredException is broken. It will throw an ArgumentException for a null fragments
		/// references, or one which contains a null reference.
		/// </summary>
		public void EnsureRulesAreMet(IEnumerable<ICSSFragment> fragments)
		{
			if (fragments == null)
				throw new ArgumentNullException("fragments");

			var firstBrokenRuleIfAny = GetAnyBrokenRules(fragments).FirstOrDefault();
			if (firstBrokenRuleIfAny != null)
				throw firstBrokenRuleIfAny;
		}

		public IEnumerable<BrokenRuleEncounteredException> GetAnyBrokenRules(IEnumerable<ICSSFragment> fragments)
		{
			if (fragments == null)
				throw new ArgumentNullException("fragments");

			foreach (var fragment in fragments)
			{
				var containerFragment = fragment as ContainerFragment;
				if (containerFragment == null)
					continue;

				var selectorFragment = fragment as Selector;
				if (selectorFragment != null)
				{
					if (!selectorFragment.IsScopeRestrictingHtmlTag() || (_scopeRestrictingHtmlTagBehaviour == ScopeRestrictingHtmlTagBehaviourOptions.Disallow))
					{
						if (selectorFragment.Selectors.Any(s => !IsValidSelector(s)))
							yield return new DisallowBareSelectorsEncounteredException(selectorFragment);
					}
				}
				foreach (var brokenRule in GetAnyBrokenRules(containerFragment.ChildFragments))
					yield return brokenRule;
			}
		}

		/// <summary>
		/// In order to be a valid selector in this context, there must be no base css selectors unless they are preceded with the child selector symbol
		/// </summary>
		private bool IsValidSelector(Selector.WhiteSpaceNormalisedString cssSelector)
		{
			if (cssSelector == null)
				throw new ArgumentNullException("cssSelector");

			// Do some minor preprocessing to make it easier to break up, align any child selector with the following selector and away from the previous
			// - We'll have to do a single replace-double-space-with-single-space in case we introduced a double space between a selector and a following
			//   child selector symbol (eg. "div.Wrapper > h2" to "div.Wrapper  >h2") but that's the only whitespace concern since the whitespace has
			//   already been normalised (as it's a WhiteSpaceNormalisedString!)
			foreach (var cssSelectorSegment in cssSelector.Value.Replace("> ", ">").Replace(">", " >").Replace("  ", " ").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
			{
				// If the segment has a child selector symbol (">"), parent selector symbol ("&") or contains a class or an id then it's ok, if none of
				// these conditions are met then the selector must be considered invalid (it is a bare selector)
				if (!cssSelectorSegment.StartsWith(">") && !cssSelectorSegment.StartsWith("&") && !cssSelectorSegment.Contains(".") && !cssSelectorSegment.Contains("#"))
					return false;
			}
			return true;
		}

		public class DisallowBareSelectorsEncounteredException : BrokenRuleEncounteredException
		{
			public DisallowBareSelectorsEncounteredException(Selector selector)
				: base(
					string.Format(
						"Disallowed bare selector encountered (\"{0}\" at line {1})",
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
