using System.Text.Json;
using Autofac.Extras.Moq;
using Logging;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using PlexRipper.Domain.Config;
using PlexRipper.Settings.Models;
using PlexRipper.Settings.Modules;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Settings.UnitTests.Modules
{
    public class DisplaySettingsModule_SetFromJson_UnitTests
    {
        public DisplaySettingsModule_SetFromJson_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public void ShouldSetPropertiesFromJson_WhenValidJsonSettingsAreGiven()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DisplaySettingsModule>();

            var settingsModel = new SettingsModel
            {
                DisplaySettings = new DisplaySettings
                {
                    MovieViewMode = ViewMode.Overview,
                    TvShowViewMode = ViewMode.Overview,
                },
            };
            var json = JsonSerializer.Serialize(settingsModel,  DefaultJsonSerializerOptions.ConfigCaptialized);
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
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DisplaySettingsModule>();

            var settingsModel = new SettingsModel
            {
                DisplaySettings = FakeData.GetDisplaySettings(new UnitTestDataConfig(234)).Generate(),
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
            _sut.TvShowViewMode.ShouldBe(_sut.DefaultValues.TvShowViewMode);
        }
    }
}