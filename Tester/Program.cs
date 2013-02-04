using System.Linq;
using CSSParser;
using NonCascadingCSSRulesEnforcer.HierarchicalParsing;

namespace Tester
{
	class Program
	{
		static void Main(string[] args)
		{
			var parsedData = (new LessCssHierarchicalParser()).ParseIntoStructuralData(
				Parser.ParseLESS(
					"// Comment\r\n\r\nbody {\r\n  > h2 { font-weight: bold; }\r\n  color: black;\r\n}\r\n\r\nbody {\r\n  background: white;\r\n}\r\n"
				).ToArray()
			);
		}
	}
}
