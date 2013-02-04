using System;
using System.Collections.Generic;
using System.Linq;

namespace NonCascadingCSSRulesEnforcer.HierarchicalParsing
{
	/// <summary>
	/// This is a CSS fragment that represents a selector string, it may be multiple comma-separated selectors (if so, then the Selectors property will have multiple entries)
	/// </summary>
	public class Selector : ICSSFragment
	{
		private readonly List<SelectorSet> _parentSelectors;
		private readonly List<ICSSFragment> _childFragments;
		public Selector(
			SelectorSet selectors,
			IEnumerable<SelectorSet> parentSelectors,
			int sourceLineIndex,
			IEnumerable<ICSSFragment> childFragments)
		{
			if (selectors == null)
				throw new ArgumentNullException("selectors");
			if (parentSelectors == null)
				throw new ArgumentNullException("parentSelectors");
			if (sourceLineIndex < 0)
				throw new ArgumentNullException("sourceLineIndex", "must be zero or greater");
			if (childFragments == null)
				throw new ArgumentNullException("childFragments");

			var parentSelectorsTidied = parentSelectors.ToList();
			if (parentSelectorsTidied.Any(s => s == null))
				throw new ArgumentException("Null reference encountered in parentSelectors set");

			var childFragmentsTidied = childFragments.ToList();
			if (childFragmentsTidied.Any(f => f == null))
				throw new ArgumentException("Null reference encountered in childFragments set");

			Selectors = selectors;
			SourceLineIndex = sourceLineIndex;
			_parentSelectors = parentSelectorsTidied;
			_childFragments = childFragmentsTidied;
		}

		/// <summary>
		/// This will never be null, empty nor contain any nulls
		/// </summary>
		public SelectorSet Selectors { get; private set; }

		/// <summary>
		/// This will never be null nor contain any nulls, it may be empty if this is a top-level Selector
		/// </summary>
		public IEnumerable<SelectorSet> ParentSelectors { get { return _parentSelectors.AsReadOnly(); } }

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
			return base.ToString() + ":" + string.Join(", ", Selectors.Selectors.Select(s => s.Value));
		}

		public class SelectorSet
		{
			private List<WhiteSpaceNormalisedString> _selectors;
			public SelectorSet(IEnumerable<WhiteSpaceNormalisedString> selectors)
			{
				if (selectors == null)
					throw new ArgumentNullException("selectors");

				var selectorsTidied = selectors.ToList();
				if (selectors.Any(s => s == null))
					throw new ArgumentException("Null reference encountered in selectors set");
				if (selectors.Any(s => s.Value.Contains(",")))
					throw new ArgumentException("Specified selectors set contains at least one entry containing a comma, selectors must be broken on commas");
				if (!selectors.Any())
					throw new ArgumentException("Empty selectors set specified");

				_selectors = selectorsTidied;
			}

			/// <summary>
			/// This will never be null, empty nor contain any nulls
			/// </summary>
			public IEnumerable<WhiteSpaceNormalisedString> Selectors { get { return _selectors.AsReadOnly(); } }
		}

		public class WhiteSpaceNormalisedString
		{
			public WhiteSpaceNormalisedString(string value)
			{
				if (string.IsNullOrWhiteSpace(value))
					throw new ArgumentException("Null/blank value specified");

				var valueTidied = new string(value.Select(c => char.IsWhiteSpace(c) ? ' ' : c).ToArray());
				while (valueTidied.Contains("  "))
					valueTidied = valueTidied.Replace("  ", " ");
				valueTidied = valueTidied.Trim();

				Value = valueTidied;
			}

			/// <summary>
			/// All whitespace characters will be replaced with spaces, than any runs of spaces will be replaced with single instances, finally
			/// the string will be trimmed. This will always have some content and never be null or blank.
			/// </summary>
			public string Value { get; private set; }

			public override string ToString()
			{
				return base.ToString() + ":" + Value;
			}
		}
	}
}
