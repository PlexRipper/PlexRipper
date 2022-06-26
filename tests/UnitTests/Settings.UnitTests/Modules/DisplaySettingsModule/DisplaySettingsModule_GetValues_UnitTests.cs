using PlexRipper.Application;
using PlexRipper.Settings.Modules;

namespace Settings.UnitTests.Modules;

public class DisplaySettingsModule_GetValues_UnitTests
{
    public DisplaySettingsModule_GetValues_UnitTests(ITestOutputHelper output)
    {
        Log.SetupTestLogging(output);
    }

    [Fact]
    public void ShouldReturnCurrentValues_WhenValidSettingsValuesAreSet()
    {
        // Arrange
        var displaySettingsModule = new DisplaySettingsModule();

        // Act
        IDisplaySettings getValuesResult = displaySettingsModule.GetValues();

        // Assert
        getValuesResult.ShouldNotBeNull();
        getValuesResult.TvShowViewMode.ShouldBe(displaySettingsModule.TvShowViewMode);
        getValuesResult.MovieViewMode.ShouldBe(displaySettingsModule.MovieViewMode);
    }
}