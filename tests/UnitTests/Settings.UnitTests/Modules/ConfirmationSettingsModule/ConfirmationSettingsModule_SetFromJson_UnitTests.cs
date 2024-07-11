using System.Text.Json;
using PlexRipper.Domain.Config;
using PlexRipper.Settings.Models;
using PlexRipper.Settings.Modules;

namespace Settings.UnitTests.Modules;

public class ConfirmationSettingsModule_SetFromJson_UnitTests : BaseUnitTest<ConfirmationSettingsModule>
{
    public ConfirmationSettingsModule_SetFromJson_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldSetPropertiesFromJson_WhenValidJsonSettingsAreGiven()
    {
        // Arrange
        var settingsModel = new SettingsModel
        {
            ConfirmationSettings = new ConfirmationSettings
            {
                AskDownloadMovieConfirmation = true,
                AskDownloadTvShowConfirmation = true,
                AskDownloadSeasonConfirmation = false,
                AskDownloadEpisodeConfirmation = false,
            },
        };
        var json = JsonSerializer.Serialize(settingsModel, DefaultJsonSerializerOptions.ConfigCaptialized);
        var loadedSettings = JsonSerializer.Deserialize<JsonElement>(
            json,
            DefaultJsonSerializerOptions.ConfigCaptialized
        );

        // Act
        var result = _sut.SetFromJson(loadedSettings);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _sut.AskDownloadMovieConfirmation.ShouldBeTrue();
        _sut.AskDownloadTvShowConfirmation.ShouldBeTrue();
        _sut.AskDownloadSeasonConfirmation.ShouldBeFalse();
        _sut.AskDownloadEpisodeConfirmation.ShouldBeFalse();
    }

    [Fact]
    public void ShouldSetPropertiesFromJson_WhenInvalidJsonSettingsAreGiven()
    {
        // Arrange
        var settingsModel = new SettingsModel
        {
            ConfirmationSettings = new ConfirmationSettings
            {
                AskDownloadMovieConfirmation = true,
                AskDownloadTvShowConfirmation = true,
                AskDownloadSeasonConfirmation = false,
                AskDownloadEpisodeConfirmation = false,
            },
        };
        var json = JsonSerializer.Serialize(settingsModel, DefaultJsonSerializerOptions.ConfigCaptialized);

        // ** Remove property to make corrupted
        json = json.Replace("AskDownloadMovieConfirmation\":true,\"", "");
        var loadedSettings = JsonSerializer.Deserialize<JsonElement>(
            json,
            DefaultJsonSerializerOptions.ConfigCaptialized
        );

        // Act
        var result = _sut.SetFromJson(loadedSettings);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _sut.AskDownloadMovieConfirmation.ShouldBeTrue();
        _sut.AskDownloadTvShowConfirmation.ShouldBeTrue();
        _sut.AskDownloadSeasonConfirmation.ShouldBeFalse();
        _sut.AskDownloadEpisodeConfirmation.ShouldBeFalse();
    }
}
