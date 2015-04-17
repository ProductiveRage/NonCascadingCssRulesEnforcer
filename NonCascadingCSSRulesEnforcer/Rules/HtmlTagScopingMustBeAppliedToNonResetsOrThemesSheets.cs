using CSSParser.ExtendedLESSParser.ContentSections;
using NonCascadingCSSRulesEnforcer.ExtendedLESSParserExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NonCascadingCSSRulesEnforcer.Rules
{
	/// <summary>
	/// This rule only applies to stylesheets other than the Resets and Themes files. No content other comments and whitespace may appear outside of scope-restricting html tags
	/// </summary>
	public class HtmlTagScopingMustBeAppliedToNonResetsOrThemesSheets : IEnforceRules
	{
		public bool DoesThisRuleApplyTo(StyleSheetTypeOptions styleSheetType)
		{
			if (!Enum.IsDefined(typeof(StyleSheetTypeOptions), styleSheetType))
				throw new ArgumentOutOfRangeException("styleSheetType");

			// This doesn't apply to Resets or Themes since they don't need html scoping and can't apply to Compiled sheets since the html-scoping tags may have been removed
			// and can't apply to Combined content since thes may include content from Resets of Themes
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

            var brokenRules = GetAnyBrokenRules(fragments);
            if (brokenRules.Any())
                throw brokenRules.First();
        }

        public IEnumerable<BrokenRuleEncounteredException> GetAnyBrokenRules(IEnumerable<ICSSFragment> fragments)
        {
            if (fragments == null)
                throw new ArgumentNullException("fragments");

            var brokenRules = new List<BrokenRuleEncounteredException>();
            foreach (var fragment in fragments)
            {
                if (fragment == null)
                    throw new ArgumentException("Encountered null fragment - invalid");

                // Ignore @import statements in non-compiled content
                if (fragment is Import)
                    continue;

                var selectorFragment = fragment as Selector;
                if ((selectorFragment == null) || !selectorFragment.IsScopeRestrictingHtmlTag())
                    yield return new ScopeRestrictingHtmlTagNotAppliedException(fragment);
            }
        }

		public class ScopeRestrictingHtmlTagNotAppliedException : BrokenRuleEncounteredException
		{
			public ScopeRestrictingHtmlTagNotAppliedException(ICSSFragment fragment) : base("Scope-restricting html tag not applied", fragment) { }
			protected ScopeRestrictingHtmlTagNotAppliedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		}
	}
}
