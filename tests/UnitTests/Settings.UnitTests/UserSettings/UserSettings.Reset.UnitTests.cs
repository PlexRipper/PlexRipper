using Reaparr.Settings.Contracts;

namespace Reaparr.Settings.UnitTests;

public class UserSettingsResetUnitTests : BaseUnitTest
{
    public UserSettingsResetUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldHaveDefaultSettingsValues_WhenResetHasBeenCalled()
    {
        // Arrange
        UserSettings sut = new();

        // Act
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
            IntegrationsSettings = IntegrationsSettings.Create(),
            AuthenticationSettings = AuthenticationModule.Create(),
            DebugSettings = DebugSettingsModule.Create(),
        };
        sut.UpdateSettings(changedSettings);
        sut.Reset();

        // Assert
        var expected = new UserSettings();

        // Assert deterministic modules against fresh defaults
        sut.AuthenticationSettings.ShouldBeEquivalentTo(expected.AuthenticationSettings);
        sut.GeneralSettings.ShouldBeEquivalentTo(expected.GeneralSettings);
        sut.ConfirmationSettings.ShouldBeEquivalentTo(expected.ConfirmationSettings);
        sut.DateTimeSettings.ShouldBeEquivalentTo(expected.DateTimeSettings);
        sut.DisplaySettings.ShouldBeEquivalentTo(expected.DisplaySettings);
        sut.DownloadManagerSettings.ShouldBeEquivalentTo(expected.DownloadManagerSettings);
        sut.LanguageSettings.ShouldBeEquivalentTo(expected.LanguageSettings);
        sut.DebugSettings.ShouldBeEquivalentTo(expected.DebugSettings);
        sut.ServerSettings.ShouldBeEquivalentTo(expected.ServerSettings);

        // IntegrationsSettings contains randomized values for some fields; assert invariants instead
        sut.IntegrationsSettings.Sonarr.ShouldBeEquivalentTo(expected.IntegrationsSettings.Sonarr);
        sut.IntegrationsSettings.Radarr.ShouldBeEquivalentTo(expected.IntegrationsSettings.Radarr);
        sut.IntegrationsSettings.DownloadClientUsername.ShouldBe(expected.IntegrationsSettings.DownloadClientUsername);

        // ReaparrApiKey should be a valid GUID string
        Guid.TryParse(sut.IntegrationsSettings.ReaparrApiKey, out _).ShouldBeTrue();

        // DownloadClientPassword should be a 32-char hex (GUID without dashes)
        sut.IntegrationsSettings.DownloadClientPassword.Length.ShouldBe(32);
    }
}
