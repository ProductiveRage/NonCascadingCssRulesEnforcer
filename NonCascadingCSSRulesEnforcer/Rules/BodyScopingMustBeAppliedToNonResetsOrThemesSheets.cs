﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CSSParser.ExtendedLESSParser;
using NonCascadingCSSRulesEnforcer.ExtendedLESSParserExtensions;

namespace NonCascadingCSSRulesEnforcer.Rules
{
	/// <summary>
	/// This rule only applies to stylesheets other than the Resets and Themes files. No content other comments and whitespace may appear outside of scope-restricting body tags
	/// </summary>
	public class BodyScopingMustBeAppliedToNonResetsOrThemesSheets : IEnforceRules
	{
		public bool DoesThisRuleApplyTo(StyleSheetTypeOptions styleSheetType)
		{
			if (!Enum.IsDefined(typeof(StyleSheetTypeOptions), styleSheetType))
				throw new ArgumentOutOfRangeException("styleSheetType");

			// This doesn't apply to Resets or Themes since they don't need body scoping and can't apply to Compiled sheets since the body-scoping tags may have been removed
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

			foreach (var fragment in fragments)
			{
				if (fragment == null)
					throw new ArgumentException("Null reference encountered in fragments set");

				// Ignore @import statements in non-compiled content
				if (fragment is Import)
					continue;

				var selectorFragment = fragment as Selector;
				if ((selectorFragment == null) || !selectorFragment.IsScopeRestrictingBodyTag())
					throw new ScopeRestrictingBodyTagNotAppliedException(fragment);
			}
		}

		public class ScopeRestrictingBodyTagNotAppliedException : BrokenRuleEncounteredException
		{
			public ScopeRestrictingBodyTagNotAppliedException(ICSSFragment fragment) : base("Scope-restricting body tag not applied", fragment) { }
			protected ScopeRestrictingBodyTagNotAppliedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		}
	}
}
