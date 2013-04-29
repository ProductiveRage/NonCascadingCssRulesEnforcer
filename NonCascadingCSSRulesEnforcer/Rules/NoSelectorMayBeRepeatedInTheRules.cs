using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CSSParser.ExtendedLESSParser;

namespace NonCascadingCSSRulesEnforcer.Rules
{
	/// <summary>
	/// TODO
	/// </summary>
	public class NoSelectorMayBeRepeatedInTheRules : IEnforceRules
	{
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

			EnsureRulesAreMet(fragments, new ContainerFragment[0]);
		}

		private void EnsureRulesAreMet(IEnumerable<ICSSFragment> fragments, IEnumerable<ContainerFragment> containers)
		{
			if (fragments == null)
				throw new ArgumentNullException("fragments");
			if (containers == null)
				throw new ArgumentNullException("containers");

			var selectors = new List<string>();
			foreach (var fragment in fragments)
			{
				if (fragment == null)
					throw new ArgumentException("Null reference encountered in fragments set");

				var selectorFragment = fragment as Selector;
				if (selectorFragment != null)
				{
					// Selectors may not be directly nested within other Selectors in compiled stylesheets, though they may appear within Media Queries. I
					// don't believe that nested Media Queries are supported by browsers currently, but it's possible that that may change in the future
					// so the only restriction that is being imposed here is against nested Selectors. This constraint makes this rule's work easier since
					// it only needs to look at Selectors within the current block. If Media Queries are nested within Selectors in LESS content then this
					// will result in the particular selector appearing multiple times in the compiled output - eg.
					//   div#Header { color: red; @media print { color: blue; } }
					// will be compiled to
					//   div#Header { color: red; }
					//   @media print { div#Header { color: red; color: blue; } }
					// and so the "div#Header" selector will appear multiple times, but not within the same block, so this is acceptable.
					var directParent = containers.LastOrDefault();
					if ((directParent != null) && (directParent is Selector))
						throw new ArgumentException("Invalid content specified - only Compiled stylesheets are applicable to this rule and Selectors may not be nested within Selectors in Compiled styles");

					foreach (var selector in selectorFragment.Selectors.Select(s => s.Value).Distinct())
					{
						if (selectors.Contains(selector))
							throw new NoSelectorMayBeRepeatedInTheRulesException(fragment, selector);
						selectors.Add(selector);
					}
				}

				var containerFragmentFragment = fragment as ContainerFragment;
				if (containerFragmentFragment != null)
				{
					EnsureRulesAreMet(containerFragmentFragment.ChildFragments, containers.Concat(new[] { containerFragmentFragment }));
					continue;
				}
			}
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
