using System.Runtime.Serialization;
using CSSParser.ExtendedLESSParser;

namespace NonCascadingCSSRulesEnforcer.Rules
{
	/// <summary>
	/// This rule applies to all stylesheets. It is very similar to the checks for fully specified margins and padding except that the important aspect of border properties are
	/// the widths and these may be specified in multiple way - eg. "border", "border-width", "border-top", "border-width-top". If any border width is specified then they all
	/// must be, one way or another.
	/// </summary>
	public class BorderWidthMustBeFullySpecifiedIfSpecifiedAtAll : PropertyMustBeFullySpecifiedIfSpecifiedAtAll
	{
		public BorderWidthMustBeFullySpecifiedIfSpecifiedAtAll()
			: base(
				new[] { "border-top", "border-width-top" },
				new[] { "border-left", "border-width-left" },
				new[] { "border-bottom", "border-width-bottom" },
				new[] { "border-right", "border-width-right" },
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
