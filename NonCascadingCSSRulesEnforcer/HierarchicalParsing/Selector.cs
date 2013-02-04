using System;
using System.Collections.Generic;
using System.Linq;

namespace NonCascadingCSSRulesEnforcer.HierarchicalParsing
{
	public class Selector : ICSSFragment
	{
		private readonly List<WhiteSpaceNormalisedString> _parentSelectors;
		private readonly List<ICSSFragment> _childFragments;
		public Selector(
			WhiteSpaceNormalisedString value,
			IEnumerable<WhiteSpaceNormalisedString> parentSelectors,
			int sourceLineIndex,
			IEnumerable<ICSSFragment> childFragments)
		{
			if (value == null)
				throw new ArgumentNullException("value");
			if (parentSelectors == null)
				throw new ArgumentNullException("parentSelectors");
			if (sourceLineIndex < 0)
				throw new ArgumentNullException("sourceLineIndex", "must be zero or greater");
			if (childFragments == null)
				throw new ArgumentNullException("childFragments");

			var parentSelectorsTidied = parentSelectors.ToList();
			if (parentSelectorsTidied.Any(f => f == null))
				throw new ArgumentException("Null reference encountered in parentSelectors set");

			var childFragmentsTidied = childFragments.ToList();
			if (childFragmentsTidied.Any(f => f == null))
				throw new ArgumentException("Null reference encountered in childFragments set");

			Value = value;
			SourceLineIndex = sourceLineIndex;
			_parentSelectors = parentSelectorsTidied;
			_childFragments = childFragmentsTidied;
		}

		/// <summary>
		/// This will never be null
		/// </summary>
		public WhiteSpaceNormalisedString Value { get; private set; }

		/// <summary>
		/// This will never be null nor contain any nulls, it may be empty if this is a top-level Selector
		/// </summary>
		public IEnumerable<WhiteSpaceNormalisedString> ParentSelectors { get { return _parentSelectors.AsReadOnly(); } }

		/// <summary>
		/// This will always be zero or greater
		/// </summary>
		public int SourceLineIndex { get; private set; }

		/// <summary>
		/// This will never be null nor contain any nulls, it may be empty if there were no child fragments for the Selector
		/// </summary>
		public IEnumerable<ICSSFragment> ChildFragments { get { return _childFragments.AsReadOnly(); } }

		public override string ToString()
		{
			return base.ToString() + ":" + Value.Value;
		}
	}
}
