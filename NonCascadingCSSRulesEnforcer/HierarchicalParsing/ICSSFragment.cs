namespace NonCascadingCSSRulesEnforcer.HierarchicalParsing
{
	public interface ICSSFragment
	{
		/// <summary>
		/// This will always be zero or greater
		/// </summary>
		int SourceLineIndex { get; }
	}
}
