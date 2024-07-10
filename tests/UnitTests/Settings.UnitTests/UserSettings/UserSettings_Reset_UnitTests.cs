using PlexRipper.Settings;
using PlexRipper.Settings.Models;
using PlexRipper.Settings.Modules;

namespace Settings.UnitTests;

public class UserSettings_Reset_UnitTests : BaseUnitTest
{
    public UserSettings_Reset_UnitTests(ITestOutputHelper output)
        : base(output) { }

    private UserSettings CreateUserSettings() =>
        new(
            _log,
            new ConfirmationSettingsModule(),
            new DateTimeSettingsModule(),
            new DisplaySettingsModule(),
            new DownloadManagerSettingsModule(),
            new GeneralSettingsModule(),
            new DebugSettingsModule(),
            new LanguageSettingsModule(),
            new ServerSettingsModule()
        );

    [Fact]
    public void ShouldHaveDefaultSettingsValues_WhenResetHasBeenCalled()
    {
        // Arrange
        var sut = CreateUserSettings();

        // Act
        var changedSettings = new SettingsModel
        {
            DateTimeSettings = new DateTimeSettings()
            {
                TimeFormat = string.Empty,
                TimeZone = string.Empty,
                LongDateFormat = string.Empty,
                ShortDateFormat = string.Empty,
            },
            ConfirmationSettings = new ConfirmationSettings()
            {
                AskDownloadEpisodeConfirmation = false,
                AskDownloadMovieConfirmation = false,
                AskDownloadSeasonConfirmation = false,
                AskDownloadTvShowConfirmation = false,
            },
            LanguageSettings = new LanguageSettings() { Language = string.Empty },
            DisplaySettings = new DisplaySettings(),
            GeneralSettings = new GeneralSettings(),
            ServerSettings = new ServerSettings(),
            DownloadManagerSettings = new DownloadManagerSettings(),
        };
        sut.UpdateSettings(changedSettings);
        sut.Reset();

        // Assert
        sut.GetSettingsModel().ShouldBeEquivalentTo(sut.GetDefaultSettingsModel());
    }
}
