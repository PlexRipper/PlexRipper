using System.Text.Json;
using Autofac.Extras.Moq;
using Logging;
using PlexRipper.Domain.Config;
using PlexRipper.Settings.Models;
using PlexRipper.Settings.Modules;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Settings.UnitTests.Modules
{
    public class ConfirmationSettingsModule_SetFromJson_UnitTests
    {
        public ConfirmationSettingsModule_SetFromJson_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public void ShouldSetPropertiesFromJson_WhenValidJsonSettingsAreGiven()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<ConfirmationSettingsModule>();

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
            var json = JsonSerializer.Serialize(settingsModel);
            var loadedSettings = JsonSerializer.Deserialize<JsonElement>(json, DefaultJsonSerializerOptions.Config);

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
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<ConfirmationSettingsModule>();

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
            var json = JsonSerializer.Serialize(settingsModel);
            // ** Remove property to make corrupted
            json = json.Replace("AskDownloadMovieConfirmation\":true,\"", "");
            var loadedSettings = JsonSerializer.Deserialize<JsonElement>(json, DefaultJsonSerializerOptions.Config);

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
}