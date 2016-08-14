using System.Linq;
using NonCascadingCSSRulesEnforcer.Rules.Compatibility;
using UnitTests.Shared;
using Xunit;

namespace UnitTests.Rules
{
	public class LegacyIESelectorLimitMustBeRespectedTests
	{
		[Fact]
		public void SingleSelectorIsFine()
		{
			var content = CSSFragmentBuilderSelector.New("div", CSSFragmentBuilderStyleProperty.New("width", "50%")).ToContainerFragment();
			LegacyIESelectorLimitMustBeRespected.Instance.EnsureRulesAreMet(new[] { content });
		}

		[Fact]
		public void FourThousandAndNinetyFiveSelectorsIsFine()
		{
			var content = Enumerable.Range(0, 4095)
				.Select(index => CSSFragmentBuilderSelector.New("div", CSSFragmentBuilderStyleProperty.New("width", "50%")).ToContainerFragment());
			LegacyIESelectorLimitMustBeRespected.Instance.EnsureRulesAreMet(content);
		}

		[Fact]
		public void FourThousandAndNinetySixSelectorsIsNotFine()
		{
			var content = Enumerable.Range(0, 4096)
				.Select(index => CSSFragmentBuilderSelector.New("div", CSSFragmentBuilderStyleProperty.New("width", "50%")).ToContainerFragment());
			Assert.Throws<LegacyIESelectorLimitMustBeRespected.LegacyIESelectorLimitMustBeRespectedException>(() =>
			{
				LegacyIESelectorLimitMustBeRespected.Instance.EnsureRulesAreMet(content);
			});
		}
	}
}
