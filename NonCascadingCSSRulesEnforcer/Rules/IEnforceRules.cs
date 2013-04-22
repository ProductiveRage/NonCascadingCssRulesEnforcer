using System.Collections.Generic;
using CSSParser.ExtendedLESSParser;

namespace NonCascadingCSSRulesEnforcer.Rules
{
	public interface IEnforceRules
	{
		/// <summary>
		/// This will throw an exception if the specified rule BrokenRuleEncounteredException is broken. It will throw an ArgumentException for a null fragments
		/// references, or one which contains a null reference.
		/// </summary>
		void EnsureRulesAreMet(IEnumerable<ICSSFragment> fragments);

		bool DoesThisRuleApplyTo(StyleSheetTypeOptions styleSheetType);
	}
}
