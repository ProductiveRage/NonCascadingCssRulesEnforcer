﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CSSParser.ExtendedLESSParser.ContentSections;
using NonCascadingCSSRulesEnforcer.ExtendedLESSParserExtensions;

namespace NonCascadingCSSRulesEnforcer.Rules
{
	/// <summary>
	/// This rule only applies to the Resets and Themes stylesheets. Only bare selectors may occur.
	/// </summary>
	public class OnlyBareSelectorsInResetsAndThemeSheets : IEnforceRules
	{
		private readonly ConformityOptions _conformity;
		public OnlyBareSelectorsInResetsAndThemeSheets(ConformityOptions conformity)
		{
			if (!Enum.IsDefined(typeof(ConformityOptions), conformity))
				throw new ArgumentOutOfRangeException("conformity");

			_conformity = conformity;
		}

		public enum ConformityOptions
		{
			/// <summary>
			/// This will allow the sheets to have LESS mixins which may appear like class selectors - eg. ".RoundedCorners (@radius)" - they must have the optional
			/// brackets after them - eg. ".RoundedCorners ()" is acceptable but ".RoundedCorners" is not - since without the brackets there is no way to differentiate
			/// between a mixin in and a class selector
			/// </summary>
			AllowLessCssMixins,
			Strict
		}

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
				var selectorFragment = fragment as Selector;
				if (selectorFragment != null)
				{
					// Mixin should contain brackets (eg. ".RounderBorders (@radius)" or ".RounderBorders ()"), while they are valid without the brackets
					// there is no way to distinguish them from class-based selectors so we can't support their detection here
					var lessCssMixin = selectorFragment.Selectors.First().Value.Contains("(");
					if (!selectorFragment.IsBareSelector() && ((_conformity == ConformityOptions.Strict) || !lessCssMixin))
						throw new OnlyAllowBareSelectorsEncounteredException(selectorFragment);
				}

				var containerFragment = fragment as ContainerFragment;
				if (containerFragment != null)
					EnsureRulesAreMet(containerFragment.ChildFragments);
			}
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
