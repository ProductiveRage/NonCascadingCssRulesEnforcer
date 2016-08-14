using System.Runtime.Serialization;
using CSSParser.ExtendedLESSParser.ContentSections;

namespace NonCascadingCSSRulesEnforcer.Rules
{
	/// <summary>
	/// This rule applies to all stylesheets. If any margin is specified, the margin must be fully specified - eg. "margin: 16px" is fine, "margin: 16px; margin-top: 8px"
	/// is fine, "margin-top: 8px; margin-left: 16px; margin-bottom: 16px; margin-right: 16px" is fine, "margin-left: 16" with the others unspecified is not.
	/// </summary>
	public class MarginMustBeFullySpecifiedIfSpecifiedAtAll : PropertyMustBeFullySpecifiedIfSpecifiedAtAll
	{
		private static MarginMustBeFullySpecifiedIfSpecifiedAtAll _instance = new MarginMustBeFullySpecifiedIfSpecifiedAtAll();
		public static MarginMustBeFullySpecifiedIfSpecifiedAtAll Instance => _instance;
		private MarginMustBeFullySpecifiedIfSpecifiedAtAll()
			: base(
				new[] { "margin-top" },
				new[] { "margin-left" },
				new[] { "margin-bottom" },
				new[] { "margin-right" },
				new[] { "margin" },
				fragment => new MarginMustBeFullySpecifiedIfSpecifiedAtAllException(fragment)
			) { }

		public class MarginMustBeFullySpecifiedIfSpecifiedAtAllException : BrokenRuleEncounteredException
		{
			public MarginMustBeFullySpecifiedIfSpecifiedAtAllException(ICSSFragment fragment) : base("Style block encountered with incomplete margin specification", fragment) { }
			protected MarginMustBeFullySpecifiedIfSpecifiedAtAllException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		}
	}
}
