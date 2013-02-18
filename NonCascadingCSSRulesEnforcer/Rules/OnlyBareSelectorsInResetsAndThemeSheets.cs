using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NonCascadingCSSRulesEnforcer.HierarchicalParsing;

namespace NonCascadingCSSRulesEnforcer.Rules
{
	/// <summary>
	/// This rule only applies to the Resets and Themes stylesheets. Only bare selectors may occur.
	/// </summary>
	public class OnlyBareSelectorsInResetsAndThemeSheets : IEnforceRules
	{
		public bool DoesThisRuleApplyTo(StyleSheetTypeOptions styleSheetType)
		{
			if (!Enum.IsDefined(typeof(StyleSheetTypeOptions), styleSheetType))
				throw new ArgumentOutOfRangeException("styleSheetType");

			return ((styleSheetType == StyleSheetTypeOptions.Reset) || (styleSheetType == StyleSheetTypeOptions.Themes));
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

				if (selectorFragment.Selectors.Any(s => !IsValidSelector(s)))
					throw new OnlyAllowBareSelectorsEncounteredException(selectorFragment);

				EnsureRulesAreMet(selectorFragment.ChildFragments);
			}
		}

		/// <summary>
		/// In order to be a valid selector in this context, there must be no base css selectors unless they are preceded with the child selector symbol
		/// </summary>
		private bool IsValidSelector(Selector.WhiteSpaceNormalisedString cssSelector)
		{
			if (cssSelector == null)
				throw new ArgumentNullException("cssSelector");

			// If it starts with a "@" then we'll assume it's a media query and let it through (whether or not media query content should be allow in
			// a Reset-or-Themes sheet is a matter for another rule)
			if (cssSelector.Value.StartsWith("@"))
				return true;

			return !cssSelector.Value.Contains('.') && !cssSelector.Value.Contains('#');
		}

		public class OnlyAllowBareSelectorsEncounteredException : BrokenRuleEncounteredException
		{
			public OnlyAllowBareSelectorsEncounteredException(Selector selector)
				: base(
					string.Format(
						"Non-bare selector encountered where this is invalid (\"{0}\" at line {1})",
						GetSelectorForDisplay(selector),
						selector.SourceLineIndex + 1),
					selector
				) { }

			protected OnlyAllowBareSelectorsEncounteredException(SerializationInfo info, StreamingContext context) : base(info, context) { }

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
