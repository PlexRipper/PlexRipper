using System.Text.Json;
using PlexRipper.Domain.Config;
using PlexRipper.Settings.Models;
using PlexRipper.Settings.Modules;

namespace Settings.UnitTests.Modules;

public class DisplaySettingsModule_SetFromJson_UnitTests : BaseUnitTest<DisplaySettingsModule>
{
    public DisplaySettingsModule_SetFromJson_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void ShouldSetPropertiesFromJson_WhenValidJsonSettingsAreGiven()
    {
        // Arrange
        var settingsModel = new SettingsModel
        {
            DisplaySettings = new DisplaySettings
            {
                MovieViewMode = ViewMode.Overview,
                TvShowViewMode = ViewMode.Overview,
            },
        };
        var json = JsonSerializer.Serialize(settingsModel, DefaultJsonSerializerOptions.ConfigCaptialized);
        var loadedSettings = JsonSerializer.Deserialize<JsonElement>(json, DefaultJsonSerializerOptions.ConfigCaptialized);

        // Act
        var result = _sut.SetFromJson(loadedSettings);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _sut.MovieViewMode.ShouldBe(ViewMode.Overview);
        _sut.TvShowViewMode.ShouldBe(ViewMode.Overview);
    }

    [Fact]
    public void ShouldSetPropertiesFromJson_WhenInvalidJsonSettingsAreGiven()
    {
        // Arrange
        var settingsModel = new SettingsModel
        {
            DisplaySettings = FakeData.GetDisplaySettings(config => { config.Seed = 234; }).Generate(),
        };
        var json = JsonSerializer.Serialize(settingsModel, DefaultJsonSerializerOptions.ConfigCaptialized);

        // ** Remove property to make corrupted
        json = json.Replace("\"TvShowViewMode\":\"Table\",", "");
        var loadedSettings = JsonSerializer.Deserialize<JsonElement>(json, DefaultJsonSerializerOptions.ConfigCaptialized);

        // Act
        var result = _sut.SetFromJson(loadedSettings);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _sut.MovieViewMode.ShouldBe(ViewMode.Overview);
        _sut.TvShowViewMode.ShouldBe(_sut.DefaultValues().TvShowViewMode);
    }
}