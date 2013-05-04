using NonCascadingCSSRulesEnforcer.Rules;
using UnitTests.Shared;
using Xunit;

namespace UnitTests.Rules
{
	public class OnlyBareSelectorsInResetsAndThemeSheetsTests
	{
		/// <summary>
		/// If brackets aren't used after the mixin name then there's no way to differentiate between it and a class selector (and the content won't be
		/// removed from the compiled output like a mixin with brackets) so bracket-less mixins are not supported
		/// </summary>
		[Fact]
		public void LESSMixinWithNoArgumentsAndNoBracketsIsNotAllowed()
		{
			var content = CSSFragmentBuilderSelector.New(
				".RoundedCorners",
				CSSFragmentBuilderStyleProperty.New("border-radius", "4px")
			).ToContainerFragment();

			Assert.Throws<OnlyBareSelectorsInResetsAndThemeSheets.OnlyAllowBareSelectorsEncounteredException>(() =>
			{
				(new OnlyBareSelectorsInResetsAndThemeSheets(OnlyBareSelectorsInResetsAndThemeSheets.ConformityOptions.AllowLessCssMixins)).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void LESSMixinWithNoArgumentsButWithBracketsIsAllowed()
		{
			var content = CSSFragmentBuilderSelector.New(
				".RoundedCorners ()",
				CSSFragmentBuilderStyleProperty.New("border-radius", "4px")
			).ToContainerFragment();

			Assert.DoesNotThrow(() =>
			{
				(new OnlyBareSelectorsInResetsAndThemeSheets(OnlyBareSelectorsInResetsAndThemeSheets.ConformityOptions.AllowLessCssMixins)).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void LESSMixinWithAnArgumentIsAllowed()
		{
			var content = CSSFragmentBuilderSelector.New(
				".RoundedCorners (@radius)",
				CSSFragmentBuilderStyleProperty.New("border-radius", "@radius")
			).ToContainerFragment();

			Assert.DoesNotThrow(() =>
			{
				(new OnlyBareSelectorsInResetsAndThemeSheets(OnlyBareSelectorsInResetsAndThemeSheets.ConformityOptions.AllowLessCssMixins)).EnsureRulesAreMet(new[] { content });
			});
		}

		[Fact]
		public void LESSMixinWithMultipleArgumentsIsAllowed()
		{
			var content = CSSFragmentBuilderSelector.New(
				".RoundedCorners (@topLeft, @topRight, @bottomRight, @bottomLeft)",
				CSSFragmentBuilderStyleProperty.New("border-radius", "@topLeft @topRight @bottomRight @bottomLeft")
			).ToContainerFragment();

			Assert.DoesNotThrow(() =>
			{
				(new OnlyBareSelectorsInResetsAndThemeSheets(OnlyBareSelectorsInResetsAndThemeSheets.ConformityOptions.AllowLessCssMixins)).EnsureRulesAreMet(new[] { content });
			});
		}
	}
}
