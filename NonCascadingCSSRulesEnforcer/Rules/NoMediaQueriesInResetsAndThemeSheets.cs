using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CSSParser.ExtendedLESSParser;

namespace NonCascadingCSSRulesEnforcer.Rules
{
	/// <summary>
	/// This rule only applies to the Resets and Themes stylesheets. No media query content may occur, this is layout that should be present in other files only.
	/// </summary>
	public class NoMediaQueriesInResetsAndThemeSheets : IEnforceRules
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

				if (selectorFragment.Selectors.Any(s => s.Value.StartsWith("@")))
					throw new NoMediaQueriesAllowedException(selectorFragment);

				EnsureRulesAreMet(selectorFragment.ChildFragments);
			}
		}

		public class NoMediaQueriesAllowedException : BrokenRuleEncounteredException
		{
			public NoMediaQueriesAllowedException(Selector selector)
				: base(
					string.Format(
						"Media query content encountered where it is invalid (\"{0}\" at line {1})",
						GetSelectorForDisplay(selector),
						selector.SourceLineIndex + 1),
					selector
				) { }

			protected NoMediaQueriesAllowedException(SerializationInfo info, StreamingContext context) : base(info, context) { }

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
