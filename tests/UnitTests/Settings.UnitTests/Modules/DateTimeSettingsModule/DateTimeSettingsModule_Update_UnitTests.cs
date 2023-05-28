using PlexRipper.Settings.Modules;

namespace Settings.UnitTests.Modules;

public class DateTimeSettingsModule_Update_UnitTests : BaseUnitTest<DateTimeSettingsModule>
{
    public DateTimeSettingsModule_Update_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void ShouldUpdateSettingsModule_WhenGivenValidSettingsObject()
    {
        // Arrange
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