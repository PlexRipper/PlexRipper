using PlexRipper.Settings.Modules;

namespace Settings.UnitTests.Modules;

public class DateTimeSettingsModule_Reset_UnitTests : BaseUnitTest<DateTimeSettingsModule>
{
    public DateTimeSettingsModule_Reset_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldUpdateAndThenResetSettingsModule_WhenCallingResetAfterUpdate()
    {
        // Arrange
        var settings = FakeData
            .GetDateTimeSettings(config =>
            {
                config.Seed = 6223;
            })
            .Generate();

        // Act
        var updateResult = _sut.Update(settings);
        var resetResult = _sut.Reset();

        // Assert
        updateResult.TimeFormat.ShouldBe(settings.TimeFormat);
        updateResult.TimeZone.ShouldBe(settings.TimeZone);
        updateResult.ShortDateFormat.ShouldBe(settings.ShortDateFormat);
        updateResult.LongDateFormat.ShouldBe(settings.LongDateFormat);
        updateResult.ShowRelativeDates.ShouldBe(settings.ShowRelativeDates);

        resetResult.TimeFormat.ShouldBe(_sut.DefaultValues().TimeFormat);
        resetResult.TimeZone.ShouldBe(_sut.DefaultValues().TimeZone);
        resetResult.ShortDateFormat.ShouldBe(_sut.DefaultValues().ShortDateFormat);
        resetResult.LongDateFormat.ShouldBe(_sut.DefaultValues().LongDateFormat);
        resetResult.ShowRelativeDates.ShouldBe(_sut.DefaultValues().ShowRelativeDates);
    }
}
