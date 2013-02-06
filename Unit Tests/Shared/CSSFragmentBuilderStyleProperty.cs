using System;

namespace UnitTests.Shared
{
	public class CSSFragmentBuilderStyleProperty : ICSSFragmentBuilderComponent
	{
		public static CSSFragmentBuilderStyleProperty New(string name, string value)
		{
			return new CSSFragmentBuilderStyleProperty(name, value);
		}

		private CSSFragmentBuilderStyleProperty(string name, string value)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Null/blank name specified");
			if (string.IsNullOrWhiteSpace(value))
				throw new ArgumentException("Null/blank value specified");

			Name = name;
			Value = value;
		}

		/// <summary>
		/// This will never be null or empty
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// This will never be null or empty
		/// </summary>
		public string Value { get; private set; }
	}
}
