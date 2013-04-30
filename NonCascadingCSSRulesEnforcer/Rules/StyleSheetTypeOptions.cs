namespace NonCascadingCSSRulesEnforcer.Rules
{
	public enum StyleSheetTypeOptions
	{
		/// <summary>
		/// The Combined content should be the entirety of the source in one string - all imports should have been flattened. There may be rules from Reset or Themes
		/// alongside Other content but it should not have been processed or compiled
		/// </summary>
		Combined,

		/// <summary>
		/// The should be the result of processing the Combined content
		/// </summary>
		Compiled,

		/// <summary>
		/// This should represent a single file's content that is not identifiable as being a Reset or Themes sheet, any imports should be left as import statements
		/// and not have their content pulled in, any processing such as LESS compilation should not have been performed
		/// </summary>
		Other,

		/// <summary>
		/// The Reset sheet and Theme sheet will have different rules applied to them than any other non-processed content (of type Other), this should represent a
		/// single file's content
		/// </summary>
		Reset,

		/// <summary>
		/// The Reset sheet and Theme sheet will have different rules applied to them than any other non-processed content (of type Other), this should represent a
		/// single file's content
		/// </summary>
		Themes
	}
}
