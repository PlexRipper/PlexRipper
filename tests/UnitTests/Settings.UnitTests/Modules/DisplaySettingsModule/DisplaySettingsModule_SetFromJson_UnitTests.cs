using System.Text.Json;
using PlexRipper.Domain.Config;
using PlexRipper.Settings;

namespace Settings.UnitTests.Modules;

public class DisplaySettingsModule_SetFromJson_UnitTests : BaseUnitTest
{
    public DisplaySettingsModule_SetFromJson_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldSetPropertiesFromJson_WhenValidJsonSettingsAreGiven()
    {
        // Arrange
        var settingsModel = new SettingsModule
        {
            DisplaySettings = new DisplaySettings { MovieViewMode = ViewMode.Table, TvShowViewMode = ViewMode.Poster },
        };
        var json = JsonSerializer.Serialize(settingsModel, DefaultJsonSerializerOptions.ConfigCapitalized);
        var loadedSettings = JsonSerializer.Deserialize<JsonElement>(
            json,
            DefaultJsonSerializerOptions.ConfigCapitalized
        );

        // Act
        var sut = new DisplaySettingsModule();
        var result = sut.SetFromJson(loadedSettings);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        sut.MovieViewMode.ShouldBe(ViewMode.Table);
        sut.TvShowViewMode.ShouldBe(ViewMode.Poster);
    }
}
