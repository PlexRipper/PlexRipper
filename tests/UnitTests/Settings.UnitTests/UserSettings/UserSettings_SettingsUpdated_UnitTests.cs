using Reaparr.Settings.Contracts;

namespace Reaparr.Settings.UnitTests;

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
        sut.SettingsUpdated.Subscribe();

        var changedSettings = new UserSettings
        {
            DateTimeSettings = DateTimeSettingsModule.Create(),
            ConfirmationSettings = new ConfirmationSettingsModule
            {
                AskDownloadEpisodeConfirmation = false,
                AskDownloadMovieConfirmation = false,
                AskDownloadSeasonConfirmation = false,
                AskDownloadTvShowConfirmation = false,
            },
            LanguageSettings = new LanguageSettingsModule { Language = string.Empty },
            DisplaySettings = DisplaySettingsModule.Create(),
            GeneralSettings = GeneralSettingsModule.Create(),
            ServerSettings = PlexServerSettingsModule.Create(),
            DownloadManagerSettings = DownloadManagerSettingsModule.Create(),
        };
        sut.UpdateSettings(changedSettings);
        sut.Reset();

        // Assert
        sut.ShouldBeEquivalentTo(new UserSettings());
    }
}
