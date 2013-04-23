using UnitTests.Rules;

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
