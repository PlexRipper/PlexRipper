using PlexRipper.Settings;
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
        UserSettings sut = new(Log);

        // Act
        var changedSettings = new SettingsModel
        {
            DateTimeSettingsModel = new DateTimeSettingsModel()
            {
                TimeFormat = string.Empty,
                TimeZone = string.Empty,
                LongDateFormat = string.Empty,
                ShortDateFormat = string.Empty,
            },
            ConfirmationSettingsModel = new ConfirmationSettingsModel()
            {
                AskDownloadEpisodeConfirmation = false,
                AskDownloadMovieConfirmation = false,
                AskDownloadSeasonConfirmation = false,
                AskDownloadTvShowConfirmation = false,
            },
            LanguageSettingsModel = new LanguageSettingsModel() { Language = string.Empty },
            DisplaySettingsModel = new DisplaySettingsModel(),
            GeneralSettingsModel = new GeneralSettingsModel(),
            ServerSettingsModel = new ServerSettingsModel(),
            DownloadManagerSettingsModel = new DownloadManagerSettingsModel(),
        };
        sut.UpdateSettings(changedSettings);
        sut.Reset();

        // Assert
        sut.GetSettingsModel().ShouldBeEquivalentTo(new SettingsModel());
    }
}
