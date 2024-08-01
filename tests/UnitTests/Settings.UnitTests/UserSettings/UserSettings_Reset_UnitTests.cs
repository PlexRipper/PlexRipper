using Settings.Contracts;

namespace Settings.UnitTests;

public class UserSettings_Reset_UnitTests : BaseUnitTest
{
    public UserSettings_Reset_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldHaveDefaultSettingsValues_WhenResetHasBeenCalled()
    {
        // Arrange
        UserSettings sut = new();

        // Act
        var changedSettings = new UserSettings
        {
            DateTimeSettings = new DateTimeSettingsModel()
            {
                TimeFormat = string.Empty,
                TimeZone = string.Empty,
                LongDateFormat = string.Empty,
                ShortDateFormat = string.Empty,
            },
            ConfirmationSettings = new ConfirmationSettingsModel()
            {
                AskDownloadEpisodeConfirmation = false,
                AskDownloadMovieConfirmation = false,
                AskDownloadSeasonConfirmation = false,
                AskDownloadTvShowConfirmation = false,
            },
            LanguageSettings = new LanguageSettingsModel() { Language = string.Empty },
            DisplaySettings = new DisplaySettingsModel(),
            GeneralSettings = new GeneralSettingsModel(),
            ServerSettings = new ServerSettingsModel(),
            DownloadManagerSettings = new DownloadManagerSettingsModel(),
        };
        sut.UpdateSettings(changedSettings);
        sut.Reset();

        // Assert
        sut.ShouldBeEquivalentTo(new UserSettings());
    }
}
