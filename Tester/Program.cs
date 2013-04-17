using System;
using NonCascadingCSSRulesEnforcer.HierarchicalParsing;
using NonCascadingCSSRulesEnforcer.Rules;
using UnitTests.Rules;
using UnitTests.Shared;

namespace Tester
{
	class Program
	{
		static void Main(string[] args)
		{
			(new AllMeasurementsMustBePixelsTests()).HundredPercentageWidthImgsAreNotAcceptableIfNotNestedInPercentageWidthDivs();
			(new AllMeasurementsMustBePixelsTests()).HundredPercentageWidthImgsMayBeAcceptableIfDeeplyNestedInPercentageWidthDivs();
			(new AllMeasurementsMustBePixelsTests()).HundredPercentageWidthImgsMayBeAcceptableIfNestedInPercentageWidthDivs();
			(new AllMeasurementsMustBePixelsTests()).NonHundredPercentageWidthImgsAreNotAcceptableIfNestedInPercentageWidthDivs();
			(new AllMeasurementsMustBePixelsTests()).PercentageWidthDivsMayBeAcceptable();
		}
	}
}
