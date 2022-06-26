using System.Text.Json;
using PlexRipper.Domain.Config;
using PlexRipper.Settings.Models;
using PlexRipper.Settings.Modules;

namespace Settings.UnitTests.Modules
{
    public class DateTimeSettingsModule_SetFromJson_UnitTests
    {
        public DateTimeSettingsModule_SetFromJson_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public void ShouldSetPropertiesFromJson_WhenValidJsonSettingsAreGiven()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var sut = mock.Create<DateTimeSettingsModule>();

            var settingsModel = new SettingsModel
            {
                DateTimeSettings = FakeData.GetDateTimeSettings(config => { config.Seed = 3246; }).Generate(),
            };
            var json = JsonSerializer.Serialize(settingsModel, DefaultJsonSerializerOptions.ConfigCaptialized);
            var loadedSettings = JsonSerializer.Deserialize<JsonElement>(json, DefaultJsonSerializerOptions.ConfigCaptialized);

            // Act
            var updateResult = sut.SetFromJson(loadedSettings);

            // Assert
            var targetSettings = settingsModel.DateTimeSettings;
            updateResult.IsSuccess.ShouldBeTrue();
            sut.TimeFormat.ShouldBe(targetSettings.TimeFormat);
            sut.TimeZone.ShouldBe(targetSettings.TimeZone);
            sut.ShortDateFormat.ShouldBe(targetSettings.ShortDateFormat);
            sut.LongDateFormat.ShouldBe(targetSettings.LongDateFormat);
            sut.ShowRelativeDates.ShouldBe(targetSettings.ShowRelativeDates);
        }

        [Fact]
        public void ShouldSetPropertiesFromJson_WhenInvalidJsonSettingsAreGiven()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var sut = mock.Create<DateTimeSettingsModule>();

            var settingsModel = new SettingsModel
            {
                DateTimeSettings = FakeData.GetDateTimeSettings(config => { config.Seed = 234; }).Generate(),
            };
            var json = JsonSerializer.Serialize(settingsModel, DefaultJsonSerializerOptions.ConfigCaptialized);

            // ** Remove property to make corrupted
            json = json.Replace($"\"{nameof(DateTimeSettings.TimeFormat)}\":\"{settingsModel.DateTimeSettings.TimeFormat}\",", "");
            var loadedSettings = JsonSerializer.Deserialize<JsonElement>(json, DefaultJsonSerializerOptions.ConfigCaptialized);

            // Act
            var updateResult = sut.SetFromJson(loadedSettings);

            // Assert
            var targetSettings = settingsModel.DateTimeSettings;
            updateResult.IsSuccess.ShouldBeTrue();

            sut.TimeFormat.ShouldBe(sut.DefaultValues().TimeFormat);

            sut.TimeZone.ShouldBe(targetSettings.TimeZone);
            sut.ShortDateFormat.ShouldBe(targetSettings.ShortDateFormat);
            sut.LongDateFormat.ShouldBe(targetSettings.LongDateFormat);
            sut.ShowRelativeDates.ShouldBe(targetSettings.ShowRelativeDates);
        }
    }
}