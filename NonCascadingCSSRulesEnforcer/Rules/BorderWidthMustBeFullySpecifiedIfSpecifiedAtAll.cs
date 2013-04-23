using System.Runtime.Serialization;
using CSSParser.ExtendedLESSParser;

namespace NonCascadingCSSRulesEnforcer.Rules
{
	/// <summary>
	/// This rule applies to all stylesheets. It is very similar to the checks for fully specified margins and padding except that the important aspect of border properties are
	/// the widths and these may be specified in multiple way - eg. "border", "border-width", "border-top", "border-top-width". If any border width is specified then they all
	/// must be, one way or another.
	/// </summary>
	public class BorderWidthMustBeFullySpecifiedIfSpecifiedAtAll : PropertyMustBeFullySpecifiedIfSpecifiedAtAll
	{
		public BorderWidthMustBeFullySpecifiedIfSpecifiedAtAll()
			: base(
				new[] { "border-top", "border-top-width" },
				new[] { "border-left", "border-left-width" },
				new[] { "border-bottom", "border-bottom-width" },
				new[] { "border-right", "border-right-width" },
				new[] { "border", "border-width" },
				fragment => new BorderWidthMustBeFullySpecifiedIfSpecifiedAtAllException(fragment)
			) { }
			

		public class BorderWidthMustBeFullySpecifiedIfSpecifiedAtAllException : BrokenRuleEncounteredException
		{
			public BorderWidthMustBeFullySpecifiedIfSpecifiedAtAllException(ICSSFragment fragment) : base("Style block encountered with incomplete border width specification", fragment) { }
			protected BorderWidthMustBeFullySpecifiedIfSpecifiedAtAllException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		}
	}
}
