﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CSSParser.ExtendedLESSParser.ContentSections;

namespace NonCascadingCSSRulesEnforcer.Rules
{
	/// <summary>
	/// This rule only applies to the Resets and Themes stylesheets. No media query content may occur, this is layout that should be present in other files only.
	/// </summary>
	public class NoMediaQueriesInResetsAndThemeSheets : IEnforceRules
	{
		private static NoMediaQueriesInResetsAndThemeSheets _instance = new NoMediaQueriesInResetsAndThemeSheets();
		public static NoMediaQueriesInResetsAndThemeSheets Instance => _instance;
		private NoMediaQueriesInResetsAndThemeSheets() { }

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
				var mediaQueryFragment = fragment as MediaQuery;
				if (mediaQueryFragment != null)
					yield return new NoMediaQueriesAllowedException(mediaQueryFragment);

				var containerFragment = fragment as ContainerFragment;
				if (containerFragment != null)
					foreach (var brokenRule in GetAnyBrokenRules(containerFragment.ChildFragments))
						yield return brokenRule;
			}
		}

		public class NoMediaQueriesAllowedException : BrokenRuleEncounteredException
		{
			public NoMediaQueriesAllowedException(MediaQuery selector)
				: base(
					string.Format(
						"Media query content encountered where it is invalid (\"{0}\" at line {1})",
						GetMediaQuerySelectorForDisplay(selector),
						selector.SourceLineIndex + 1),
					selector
				) { }

			protected NoMediaQueriesAllowedException(SerializationInfo info, StreamingContext context) : base(info, context) { }

			private static string GetMediaQuerySelectorForDisplay(MediaQuery mediaQuery)
			{
				if (mediaQuery == null)
					throw new ArgumentNullException("mediaQuery");

				return string.Join(
					", ",
					mediaQuery.Selectors.Select(s => s.Value)
				);
			}
		}
	}
}
