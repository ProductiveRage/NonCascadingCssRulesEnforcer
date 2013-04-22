using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CSSParser.ExtendedLESSParser;

namespace NonCascadingCSSRulesEnforcer.Rules
{
	/// <summary>
	/// This rule applies to all stylesheets. If any margin is specified, the margin must be fully specified - eg. "margin: 16px" is fine, "margin: 16px; margin-top: 8px"
	/// is fine, "margin-top: 8px; margin-left: 16px; margin-bottom: 16px; margin-right: 16px" is fine, "margin-left: 16" with the others unspecified is not.
	/// </summary>
	public class MarginsMustBeFullySpecifiedIfSpecifiedAtAll : IEnforceRules
	{
		public bool DoesThisRuleApplyTo(StyleSheetTypeOptions styleSheetType)
		{
			if (!Enum.IsDefined(typeof(StyleSheetTypeOptions), styleSheetType))
				throw new ArgumentOutOfRangeException("styleSheetType");

			return true;
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
				var containerFragment = fragment as ContainerFragment;
				if (containerFragment == null)
					continue;

				var stylePropertyNames = containerFragment.ChildFragments.Where(f => f is StylePropertyName).Cast<StylePropertyName>().Select(s => s.Value.ToLower());
				var possiblyPartiallyDefined =
					stylePropertyNames.Contains("margin-top") ||
					stylePropertyNames.Contains("margin-left") ||
					stylePropertyNames.Contains("margin-bottom") ||
					stylePropertyNames.Contains("margin-right");
				if (possiblyPartiallyDefined)
				{
					var implicitlyFullyDefined = stylePropertyNames.Contains("margin");
					var explicitlyFullyDefined =
						stylePropertyNames.Contains("margin-top") &&
						stylePropertyNames.Contains("margin-left") &&
						stylePropertyNames.Contains("margin-bottom") &&
						stylePropertyNames.Contains("margin-right");
					if (!implicitlyFullyDefined && !explicitlyFullyDefined)
						throw new MarginsMustBeFullySpecifiedIfSpecifiedAtAllException(containerFragment);
				}
				
				EnsureRulesAreMet(containerFragment.ChildFragments);
			}
		}

		public class MarginsMustBeFullySpecifiedIfSpecifiedAtAllException : BrokenRuleEncounteredException
		{
			public MarginsMustBeFullySpecifiedIfSpecifiedAtAllException(ICSSFragment fragment) : base("Style block encountered with incomplete margin specification", fragment) { }
			protected MarginsMustBeFullySpecifiedIfSpecifiedAtAllException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		}
	}
}
