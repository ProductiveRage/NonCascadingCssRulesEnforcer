using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CSSParser.ExtendedLESSParser;
using NonCascadingCSSRulesEnforcer.ExtendedLESSParserExtensions;

namespace NonCascadingCSSRulesEnforcer.Rules
{
	/// <summary>
	/// No style block may specify a width with a non-zero padding or border width
	/// </summary>
	public class BorderAndPaddingMayNotBeCombinedWithWidth : IEnforceRules
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

				// If a width is specified..
				// - Although there could be multiple "width" properties specified, in which case only the last would be applied by the browser, this is not
				//   a case I'm interested in supporting since there should be only a single width property (if any) for the sake of clarity (only need to
				//   consider ones that have measurement values since they will either have a measurement value or be "auto" or "inherit" or invalid,
				//   the last three of which we don't have to worry about)
				var isWidthDefined = containerFragment.ChildFragments
					.Where(f => (f is StylePropertyValue))
					.Cast<StylePropertyValue>()
					.Where(s => s.Property.Value.Equals("width", StringComparison.InvariantCultureIgnoreCase))
					.Any(s => s.GetValueSectionsThatAreMeasurements().Any());
				if (isWidthDefined)
				{
					// .. and a padding or border other than zero (which is checked for explicitly while a border value of "none" is ignored entirely)..
					var firstInvalidProperty = containerFragment.ChildFragments
						.Where(f => f is StylePropertyValue)
						.Cast<StylePropertyValue>()
						.Where(v =>
							v.Property.Value.Equals("padding", StringComparison.InvariantCultureIgnoreCase) ||
							v.Property.Value.StartsWith("border", StringComparison.InvariantCultureIgnoreCase)
						)
						.Where(v => v.GetValueSectionsThatAreMeasurements().Any(m => m.Value != 0))
						.FirstOrDefault();

					// .. then throw a broken rule exception
					if (firstInvalidProperty != null)
						throw new BorderAndPaddingMayNotBeCombinedWithWidthException(firstInvalidProperty);
				}
				
				EnsureRulesAreMet(containerFragment.ChildFragments);
			}
		}
		
		public class BorderAndPaddingMayNotBeCombinedWithWidthException : BrokenRuleEncounteredException
		{
			public BorderAndPaddingMayNotBeCombinedWithWidthException(ICSSFragment fragment) : base("Style block encountered that combines border and/or padding with width", fragment) { }
			protected BorderAndPaddingMayNotBeCombinedWithWidthException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		}
	}
}
