using System;
using System.Runtime.Serialization;
using NonCascadingCSSRulesEnforcer.HierarchicalParsing;

namespace NonCascadingCSSRulesEnforcer.Rules
{
	public class BrokenRuleEncounteredException : Exception
	{
		public BrokenRuleEncounteredException(string message, ICSSFragment fragment) : base((message ?? "").Trim())
		{
			if (string.IsNullOrWhiteSpace(message))
				throw new ArgumentException("Null/blank message specified");

			Fragment = fragment;
		}

		protected BrokenRuleEncounteredException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			Fragment = (ICSSFragment)info.GetValue("Fragment", typeof(ICSSFragment));
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Fragment", Fragment);
			base.GetObjectData(info, context);
		}

		/// <summary>
		/// This will never be null
		/// </summary>
		public ICSSFragment Fragment { get; private set; }
	}
}
