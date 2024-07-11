using PlexRipper.Settings.Modules;

namespace Settings.UnitTests.Modules;

public class DisplaySettingsModule_GetValues_UnitTests : BaseUnitTest
{
    public DisplaySettingsModule_GetValues_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldReturnCurrentValues_WhenValidSettingsValuesAreSet()
    {
        // Arrange
        var displaySettingsModule = new DisplaySettingsModule();

        // Act
        var getValuesResult = displaySettingsModule.GetValues();

        // Assert
        getValuesResult.ShouldNotBeNull();
        getValuesResult.TvShowViewMode.ShouldBe(displaySettingsModule.TvShowViewMode);
        getValuesResult.MovieViewMode.ShouldBe(displaySettingsModule.MovieViewMode);
    }
}
