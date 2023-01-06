using System.Text.Json;
using PlexRipper.Domain.Config;
using PlexRipper.Settings.Models;
using PlexRipper.Settings.Modules;

namespace Settings.UnitTests.Modules;

public class DateTimeSettingsModule_SetFromJson_UnitTests : BaseUnitTest<DateTimeSettingsModule>
{
    public DateTimeSettingsModule_SetFromJson_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void ShouldSetPropertiesFromJson_WhenValidJsonSettingsAreGiven()
    {
        // Arrange
        var settingsModel = new SettingsModel
        {
            DateTimeSettings = FakeData.GetDateTimeSettings(config => { config.Seed = 3246; }).Generate(),
        };
        var json = JsonSerializer.Serialize(settingsModel, DefaultJsonSerializerOptions.ConfigCaptialized);
        var loadedSettings = JsonSerializer.Deserialize<JsonElement>(json, DefaultJsonSerializerOptions.ConfigCaptialized);

        // Act
        var updateResult = _sut.SetFromJson(loadedSettings);

        // Assert
        var targetSettings = settingsModel.DateTimeSettings;
        updateResult.IsSuccess.ShouldBeTrue();
        _sut.TimeFormat.ShouldBe(targetSettings.TimeFormat);
        _sut.TimeZone.ShouldBe(targetSettings.TimeZone);
        _sut.ShortDateFormat.ShouldBe(targetSettings.ShortDateFormat);
        _sut.LongDateFormat.ShouldBe(targetSettings.LongDateFormat);
        _sut.ShowRelativeDates.ShouldBe(targetSettings.ShowRelativeDates);
    }

    [Fact]
    public void ShouldSetPropertiesFromJson_WhenInvalidJsonSettingsAreGiven()
    {
        // Arrange
        var settingsModel = new SettingsModel
        {
            DateTimeSettings = FakeData.GetDateTimeSettings(config => { config.Seed = 234; }).Generate(),
        };
        var json = JsonSerializer.Serialize(settingsModel, DefaultJsonSerializerOptions.ConfigCaptialized);

        // ** Remove property to make corrupted
        json = json.Replace($"\"{nameof(DateTimeSettings.TimeFormat)}\":\"{settingsModel.DateTimeSettings.TimeFormat}\",", "");
        var loadedSettings = JsonSerializer.Deserialize<JsonElement>(json, DefaultJsonSerializerOptions.ConfigCaptialized);

        // Act
        var updateResult = _sut.SetFromJson(loadedSettings);

        // Assert
        var targetSettings = settingsModel.DateTimeSettings;
        updateResult.IsSuccess.ShouldBeTrue();

        _sut.TimeFormat.ShouldBe(_sut.DefaultValues().TimeFormat);

        _sut.TimeZone.ShouldBe(targetSettings.TimeZone);
        _sut.ShortDateFormat.ShouldBe(targetSettings.ShortDateFormat);
        _sut.LongDateFormat.ShouldBe(targetSettings.LongDateFormat);
        _sut.ShowRelativeDates.ShouldBe(targetSettings.ShowRelativeDates);
    }
}