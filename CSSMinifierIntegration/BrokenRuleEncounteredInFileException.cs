using System;
using System.Runtime.Serialization;

namespace NonCascadingCSSRulesEnforcer.Rules
{
	[Serializable]
	public class BrokenRuleEncounteredInFileException : Exception
	{
		public BrokenRuleEncounteredInFileException(
			BrokenRuleEncounteredException brokenRuleException,
			StyleSheetTypeOptions styleSheetType,
			string relativePath) : base(TryToGetMessage(brokenRuleException, styleSheetType, relativePath) ?? "")
		{
			if (brokenRuleException == null)
				throw new ArgumentNullException("brokenRuleException");
			if (!Enum.IsDefined(typeof(StyleSheetTypeOptions), styleSheetType))
				throw new ArgumentOutOfRangeException("styleSheetType");
			if (string.IsNullOrWhiteSpace(relativePath))
				throw new ArgumentException("Null/blank relativePath specified");

			BrokenRuleException = brokenRuleException;
			StyleSheetType = styleSheetType;
			RelativePath = relativePath;
		}

		/// <summary>
		/// Try to get a message value for the base Exception constructor. If any of the arguments are invalid then return null and allow the constructor
		/// above to throw an ArgumentException when it identifies them. This should combine the brokenRuleException's message with the filename of the
		/// source file (if the content is Combined or Compiled then indicate this, likewise if it's identified as a Resets or Theme sheet). If the
		/// file is not the result of Combined or Compiled content then also include the line number which was identified as invalid (it won't
		/// mean anything for Combined or Compiled content, so don't bother for those).
		/// </summary>
		private static string TryToGetMessage(
			BrokenRuleEncounteredException brokenRuleException,
			StyleSheetTypeOptions styleSheetType,
			string relativePath)
		{
			if ((brokenRuleException == null) || !Enum.IsDefined(typeof(StyleSheetTypeOptions), styleSheetType) || string.IsNullOrWhiteSpace(relativePath))
				return null;

			var filename = relativePath;
			if (styleSheetType != StyleSheetTypeOptions.Other)
				filename += "[" + styleSheetType + "]";
			
			var message = brokenRuleException.Message + " in " + filename;
			if ((styleSheetType != StyleSheetTypeOptions.Combined) && (styleSheetType != StyleSheetTypeOptions.Compiled))
			{
				// SourceLineIndex is zero-based so add one to show line number
				message += " (line " + (brokenRuleException.Fragment.SourceLineIndex + 1) + ")";
			}
			return message;
		}

		protected BrokenRuleEncounteredInFileException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			BrokenRuleException = (BrokenRuleEncounteredException)info.GetValue("Fragment", typeof(BrokenRuleEncounteredException));
			StyleSheetType = (StyleSheetTypeOptions)info.GetValue("StyleSheetType", typeof(StyleSheetTypeOptions));
			RelativePath = info.GetString("RelativePath");
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("BrokenRuleException", BrokenRuleException);
			info.AddValue("StyleSheetType", StyleSheetType);
			info.AddValue("RelativePath", RelativePath);
			base.GetObjectData(info, context);
		}

		/// <summary>
		/// This will never be null
		/// </summary>
		public BrokenRuleEncounteredException BrokenRuleException { get; private set; }

		public StyleSheetTypeOptions StyleSheetType { get; private set; }

		/// <summary>
		/// This will never be null or blank
		/// </summary>
		public string RelativePath { get; private set; }
	}
}
