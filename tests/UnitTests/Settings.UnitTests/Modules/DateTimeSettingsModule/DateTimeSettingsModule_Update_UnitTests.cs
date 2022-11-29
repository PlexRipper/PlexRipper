using PlexRipper.Settings.Modules;

namespace Settings.UnitTests.Modules;

public class DateTimeSettingsModule_Update_UnitTests
{
    public DateTimeSettingsModule_Update_UnitTests(ITestOutputHelper output)
    {
        Log.SetupTestLogging(output);
    }

    [Fact]
    public void ShouldUpdateSettingsModule_WhenGivenValidSettingsObject()
    {
        // Arrange
        using var mock = AutoMock.GetStrict();
        var _sut = mock.Create<DateTimeSettingsModule>();
        var settings = FakeData.GetDateTimeSettings(config => { config.Seed = 6223; }).Generate();

        // Act
        var updateResult = _sut.Update(settings);

        // Assert
        updateResult.TimeFormat.ShouldBe(settings.TimeFormat);
        updateResult.TimeZone.ShouldBe(settings.TimeZone);
        updateResult.ShortDateFormat.ShouldBe(settings.ShortDateFormat);
        updateResult.LongDateFormat.ShouldBe(settings.LongDateFormat);
        updateResult.ShowRelativeDates.ShouldBe(settings.ShowRelativeDates);
    }
}