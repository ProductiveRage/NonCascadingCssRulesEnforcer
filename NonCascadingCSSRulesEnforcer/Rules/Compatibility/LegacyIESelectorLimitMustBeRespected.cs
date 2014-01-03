using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CSSParser.ExtendedLESSParser.ContentSections;

namespace NonCascadingCSSRulesEnforcer.Rules.Compatibility
{
	/// <summary>
	/// This rule is not about validating the quality of stylesheets but ensuring that the IE selector limit (for versions before 10) is not exceeded - after 4095, legacy IE
	/// versions will silently ignore rules. This can be problematic if it's not caught quickly (since the styles that are ignored may or may not be obvious). This can only
	/// apply to the fully compiled stylesheet content.
	/// </summary>
	public class LegacyIESelectorLimitMustBeRespected : IEnforceRules
	{
		private const int MAX_NUMBER_OF_SELECTORS = 4095;

		public bool DoesThisRuleApplyTo(StyleSheetTypeOptions styleSheetType)
		{
			if (!Enum.IsDefined(typeof(StyleSheetTypeOptions), styleSheetType))
				throw new ArgumentOutOfRangeException("styleSheetType");

			return (styleSheetType == StyleSheetTypeOptions.Compiled);
		}

		/// <summary>
		/// This will throw an exception if the specified rule BrokenRuleEncounteredException is broken. It will throw an ArgumentException for a null fragments
		/// references, or one which contains a null reference.
		/// </summary>
		public void EnsureRulesAreMet(IEnumerable<ICSSFragment> fragments)
		{
			if (fragments == null)
				throw new ArgumentNullException("fragments");

			var fragmentSet = new CSSFragmentSet(fragments);
			if (fragmentSet.TotalSelectorCount > MAX_NUMBER_OF_SELECTORS)
				throw new LegacyIESelectorLimitMustBeRespectedException(fragmentSet);
		}

		public class LegacyIESelectorLimitMustBeRespectedException : BrokenRuleEncounteredException
		{
			public LegacyIESelectorLimitMustBeRespectedException(CSSFragmentSet fragmentSet) : base(GetMessage(fragmentSet, null), fragmentSet) { }
			public LegacyIESelectorLimitMustBeRespectedException(string additionalMessageContent, CSSFragmentSet fragmentSet)
				: base(GetMessage(fragmentSet, additionalMessageContent), fragmentSet) { }

			protected LegacyIESelectorLimitMustBeRespectedException(SerializationInfo info, StreamingContext context) : base(info, context) { }

			private static string GetMessage(CSSFragmentSet fragmentSet, string optionalAdditionalMessageContent)
			{
				if (fragmentSet == null)
					throw new ArgumentNullException("fragmentSet");

				var message = string.Format(
					"Legacy IE (pre-v10) Selector Limit ({0}) exceeded ({1})",
					MAX_NUMBER_OF_SELECTORS,
					fragmentSet.TotalSelectorCount
				);
				if (!string.IsNullOrWhiteSpace(optionalAdditionalMessageContent))
					message += ": " + optionalAdditionalMessageContent;
				return message;
			}
		}

		public class CSSFragmentSet : ICSSFragment
		{
			public CSSFragmentSet(IEnumerable<ICSSFragment> fragments)
			{
				if (fragments == null)
					throw new ArgumentNullException("fragments");

				Fragments = fragments.ToList().AsReadOnly();
				if (Fragments.Any(f => f == null))
					throw new ArgumentException("Null reference encountered in fragments set");

				TotalSelectorCount = GetSelectors(Fragments).Select(s => s.Selectors.Count()).Sum();
			}

			/// <summary>
			/// This will never be null nor contain any null references
			/// </summary>
			public IEnumerable<ICSSFragment> Fragments { get; private set; }

			/// <summary>
			/// This will always be zero or greater
			/// </summary>
			public int TotalSelectorCount { get; private set; }

			/// <summary>
			/// This will always be zero or greater
			/// </summary>
			public int SourceLineIndex { get { return 0; } } // This isn't really important here so just return zero

			private static IEnumerable<Selector> GetSelectors(IEnumerable<ICSSFragment> fragments)
			{
				if (fragments == null)
					throw new ArgumentNullException("fragments");

				var selectors = new List<Selector>();
				foreach (var fragment in fragments)
				{
					if (fragment == null)
						throw new ArgumentException("Null reference encountered in fragments set");

					var selectorFragment = fragment as Selector;
					if (selectorFragment != null)
						selectors.Add(selectorFragment);

					var containerFragment = fragment as ContainerFragment;
					if (containerFragment != null)
						selectors.AddRange(GetSelectors(containerFragment.ChildFragments));
				}
				return selectors;
			}
		}
	}
}
