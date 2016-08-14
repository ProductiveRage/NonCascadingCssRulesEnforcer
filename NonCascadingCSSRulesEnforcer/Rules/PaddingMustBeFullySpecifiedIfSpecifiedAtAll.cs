using System.Runtime.Serialization;
using CSSParser.ExtendedLESSParser.ContentSections;

namespace NonCascadingCSSRulesEnforcer.Rules
{
	/// <summary>
	/// This rule applies to all stylesheets. If any padding is specified, the padding must be fully specified - eg. "padding: 16px" is fine, "padding: 16px; padding-top: 8px"
	/// is fine, "padding-top: 8px; padding-left: 16px; padding-bottom: 16px; padding-right: 16px" is fine, "padding-left: 16" with the others unspecified is not.
	/// </summary>
	public class PaddingMustBeFullySpecifiedIfSpecifiedAtAll : PropertyMustBeFullySpecifiedIfSpecifiedAtAll
	{
		private static PaddingMustBeFullySpecifiedIfSpecifiedAtAll _instance = new PaddingMustBeFullySpecifiedIfSpecifiedAtAll();
		public static PaddingMustBeFullySpecifiedIfSpecifiedAtAll Instance => _instance;
		private PaddingMustBeFullySpecifiedIfSpecifiedAtAll()
			: base(
				new[] { "padding-top" },
				new[] { "padding-left" },
				new[] { "padding-bottom" },
				new[] { "padding-right" },
				new[] { "padding" },
				fragment => new PaddingMustBeFullySpecifiedIfSpecifiedAtAllException(fragment)
			) { }
			

		public class PaddingMustBeFullySpecifiedIfSpecifiedAtAllException : BrokenRuleEncounteredException
		{
			public PaddingMustBeFullySpecifiedIfSpecifiedAtAllException(ICSSFragment fragment) : base("Style block encountered with incomplete padding specification", fragment) { }
			protected PaddingMustBeFullySpecifiedIfSpecifiedAtAllException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		}
	}
}
