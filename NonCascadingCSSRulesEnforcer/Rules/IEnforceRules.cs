using System.Collections.Generic;
using CSSParser.ExtendedLESSParser.ContentSections;

namespace NonCascadingCSSRulesEnforcer.Rules
{
	public interface IEnforceRules
	{
		/// <summary>
		/// This will throw an exception if the specified rule BrokenRuleEncounteredException is broken. It will throw an ArgumentException for a null fragments
		/// references, or one which contains a null reference.
		/// </summary>
		void EnsureRulesAreMet(IEnumerable<ICSSFragment> fragments);

        /// <summary>
        /// This will never return nor, nor a set containing any nulls. If the provided content does not break any rules that this implementation is responsible
        /// for then this will return an empty set. It will throw an ArgumentException for a null fragments references, or one which contains a null reference.
        /// </summary>
        IEnumerable<BrokenRuleEncounteredException> GetAnyBrokenRules(IEnumerable<ICSSFragment> fragments);

		bool DoesThisRuleApplyTo(StyleSheetTypeOptions styleSheetType);
	}
}
