using System.Text.Json;
using PlexRipper.Domain.Config;
using PlexRipper.Settings.Models;
using PlexRipper.Settings.Modules;

namespace Settings.UnitTests.Modules;

public class DisplaySettingsModule_SetFromJson_UnitTests : BaseUnitTest
{
    public DisplaySettingsModule_SetFromJson_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldSetPropertiesFromJson_WhenValidJsonSettingsAreGiven()
    {
        // Arrange
        var settingsModel = new SettingsModel
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

    [Fact]
    public void ShouldSetPropertiesFromJson_WhenInvalidJsonSettingsAreGiven()
    {
        // Arrange
        var settingsModel = new SettingsModel
        {
            DisplaySettings = FakeData
                .GetDisplaySettings(config =>
                {
                    config.Seed = 234;
                })
                .Generate(),
        };
        var json = JsonSerializer.Serialize(settingsModel, DefaultJsonSerializerOptions.ConfigCapitalized);

        // ** Remove property to make corrupted
        json = json.Replace("\"TvShowViewMode\":\"Table\",", "");
        var loadedSettings = JsonSerializer.Deserialize<JsonElement>(
            json,
            DefaultJsonSerializerOptions.ConfigCapitalized
        );

        // Act
        var sut = new DisplaySettingsModule();
        var result = sut.SetFromJson(loadedSettings);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        sut.MovieViewMode.ShouldBe(ViewMode.Poster);
        sut.TvShowViewMode.ShouldBe(sut.DefaultValues().TvShowViewMode);
    }
}
