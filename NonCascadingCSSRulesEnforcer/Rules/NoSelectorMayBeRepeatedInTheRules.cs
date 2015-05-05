using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CSSParser.ExtendedLESSParser.ContentSections;
using NonCascadingCSSRulesEnforcer.ExtendedLESSParserExtensions;

namespace NonCascadingCSSRulesEnforcer.Rules
{
	/// <summary>
	/// No selectors should be repeated throughout the rules, each element should be fully defined in a single place. LESS mixins may be used to declare styles that
	/// are applied to multiple elements if some of those elements require further specialisation. The rule can optionally be relaxed for bare selectors so that
	/// selectors in Resets or Themes may be repeated (eg. "strong" may be reset in a standard Reset sheet and then overwritten with "font-weight:bold" in a
	/// given Theme sheet).
	/// </summary>
	public class NoSelectorMayBeRepeatedInTheRules : IEnforceRules
	{
		private readonly ConformityOptions _conformity;
		public NoSelectorMayBeRepeatedInTheRules(ConformityOptions conformity)
		{
			if (!Enum.IsDefined(typeof(ConformityOptions), conformity))
				throw new ArgumentOutOfRangeException("conformity");

			_conformity = conformity;
		}

		public enum ConformityOptions
		{
			/// <summary>
			/// Bare selectors may optionally be allowed, this is to allow for Resets and Themes sheets to both have basic styles for some elements
			/// </summary>
			AllowBareSelectorsToBeRepeated,

			/// <summary>
			/// No selectors may be repeated if this option is enabled
			/// </summary>
			Strict
		}

		public bool DoesThisRuleApplyTo(StyleSheetTypeOptions styleSheetType)
		{
			if (!Enum.IsDefined(typeof(StyleSheetTypeOptions), styleSheetType))
				throw new ArgumentOutOfRangeException("styleSheetType");

			// Applying this to individual stylesheets would not give adequate cover (since selectors shouldn't be repeated anywhere throughout the styles, not
			// just within a given sheet). It can't be applied to compiled content since it would prevent valid DRY through using of LESS mixins - eg.
			//
			//   .ListWithBlueItems
			//   {
			//     > li { color: blue; }
			//   }
			//   ul.List1
			//   {
			//     .ListWithBlueItems;
			//     > li { padding: 4px; }
			//   }
			//
			// will be compiled into
			//
			//   ul.List1 > li { color: blue; }
			//   ul.List1 > li { padding: 4px; }
			//
			// resulting in repeated selectors even though effort has been made to avoid duplication in the source.
			// So this rule can only be applied Combined stylesheet content.
			return (styleSheetType == StyleSheetTypeOptions.Combined);
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

            // TODO: (Optionally?) allow mixins to be repeated(?)

            var allSelectors = GetAllSelectors(fragments, new ContainerFragment.SelectorSet[0]);
            if (_conformity == ConformityOptions.AllowBareSelectorsToBeRepeated)
                allSelectors = allSelectors.Where(s => !s.Selector.OnlyTargetsBareSelectors());

            var usedSelectorLookup = new Dictionary<string, ICSSFragment>();
            foreach (var selector in allSelectors.Select(s => new { Source = s.Source, Value = string.Join(" ", s.Selector.Select(v => v.Value)) }))
            {
                if (usedSelectorLookup.ContainsKey(selector.Value))
                {
                    yield return new NoSelectorMayBeRepeatedInTheRulesException(usedSelectorLookup[selector.Value], selector.Value);
                    continue;
                }
                usedSelectorLookup.Add(selector.Value, selector.Source);
            }
        }

		private IEnumerable<SelectorSetWithSourceFragment> GetAllSelectors(IEnumerable<ICSSFragment> fragments, IEnumerable<ContainerFragment.SelectorSet> parentSelectors)
		{
			if (fragments == null)
				throw new ArgumentNullException("fragments");
			if (parentSelectors == null)
				throw new ArgumentNullException("parentSelectors");

			var newSelectors = new List<SelectorSetWithSourceFragment>();
			foreach (var fragment in fragments)
			{
				if (fragment == null)
					throw new ArgumentException("Null reference encountered in fragments set");

				var containerFragment = fragment as ContainerFragment;
				if (containerFragment == null)
					continue;

				var parentSelectorsForChildFragments = new List<ContainerFragment.SelectorSet>();
				if (!parentSelectors.Any())
				{
					parentSelectorsForChildFragments = containerFragment.Selectors.Select(
						s => new ContainerFragment.SelectorSet(new[] { s })
					).ToList();
				}
				else
				{
					foreach (var parentSelector in parentSelectors)
					{
						if (parentSelector == null)
							throw new ArgumentException("Null reference encountered in parentSelectors set");
						foreach (var containerFragmentSelector in containerFragment.Selectors)
						{
							parentSelectorsForChildFragments.Add(
								new ContainerFragment.SelectorSet(parentSelector.Concat(new[] { containerFragmentSelector }))
							);
						}
					}
				}

				if (containerFragment.ChildFragments.Any(f => f is StylePropertyValue))
				{
					newSelectors.AddRange(
						parentSelectorsForChildFragments.Select(s => new SelectorSetWithSourceFragment(s, containerFragment))
					);
				}

				newSelectors.AddRange(
					GetAllSelectors(containerFragment.ChildFragments, parentSelectorsForChildFragments)
				);
			}
			return newSelectors;
		}

		private class SelectorSetWithSourceFragment
		{
			public SelectorSetWithSourceFragment(ContainerFragment.SelectorSet selector, ICSSFragment source)
			{
				if (selector == null)
					throw new ArgumentNullException("selector");
				if (source == null)
					throw new ArgumentNullException("source");

				Selector = selector;
				Source = source;
			}

			/// <summary>
			/// This will never be null
			/// </summary>
			public ContainerFragment.SelectorSet Selector { get; private set; }

			/// <summary>
			/// This will never be null
			/// </summary>
			public ICSSFragment Source { get; private set; }
		}

		public class NoSelectorMayBeRepeatedInTheRulesException : BrokenRuleEncounteredException
		{
			public NoSelectorMayBeRepeatedInTheRulesException(ICSSFragment fragment, string selector) : base("Selector encountered multiple times: " + (selector ?? ""), fragment)
			{
				if (string.IsNullOrWhiteSpace(selector))
					throw new ArgumentException("Null/blank selector specified");

				Selector = selector.Trim();
			}
			
			protected NoSelectorMayBeRepeatedInTheRulesException(SerializationInfo info, StreamingContext context) : base(info, context) { }

			public override void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				info.AddValue("Selector", Selector);
				base.GetObjectData(info, context);
			}

			/// <summary>
			/// This will never be null or blank
			/// </summary>
			public string Selector { get; private set; }
		}
	}
}
