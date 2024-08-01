using Settings.Contracts;

namespace Settings.UnitTests;

public class UserSettings_SettingsUpdated_UnitTests : BaseUnitTest
{
    public UserSettings_SettingsUpdated_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldHaveDefaultSettingsValues_WhenResetHasBeenCalled()
    {
        // Arrange
        UserSettings sut = new();

        // Act
        List<UserSettings> settings = new();
        sut.SettingsUpdated.Subscribe();

        var changedSettings = new UserSettings
        {
            DateTimeSettings = new DateTimeSettingsModule()
            {
                TimeFormat = string.Empty,
                TimeZone = string.Empty,
                LongDateFormat = string.Empty,
                ShortDateFormat = string.Empty,
            },
            ConfirmationSettings = new ConfirmationSettingsModule()
            {
                AskDownloadEpisodeConfirmation = false,
                AskDownloadMovieConfirmation = false,
                AskDownloadSeasonConfirmation = false,
                AskDownloadTvShowConfirmation = false,
            },
            LanguageSettings = new LanguageSettingsModule() { Language = string.Empty },
            DisplaySettings = new DisplaySettingsModule(),
            GeneralSettings = new GeneralSettingsModule(),
            ServerSettings = new PlexServerSettingsModule(),
            DownloadManagerSettings = new DownloadManagerSettingsModule(),
        };
        sut.UpdateSettings(changedSettings);
        sut.Reset();

        // Assert
        sut.ShouldBeEquivalentTo(new UserSettings());
    }
}
