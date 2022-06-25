using Logging;
using PlexRipper.Settings;
using PlexRipper.Settings.Models;
using PlexRipper.Settings.Modules;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Settings.UnitTests
{
    public class UserSettings_Reset_UnitTests
    {
        public UserSettings_Reset_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        private UserSettings CreateUserSettings()
        {
            return new UserSettings(
                new ConfirmationSettingsModule(),
                new DateTimeSettingsModule(),
                new DisplaySettingsModule(),
                new DownloadManagerSettingsModule(),
                new GeneralSettingsModule(),
                new LanguageSettingsModule(),
                new ServerSettingsModule());
        }

        [Fact]
        public void ShouldHaveDefaultSettingsValues_WhenResetHasBeenCalled()
        {
            // Arrange
            var sut = CreateUserSettings();

            // Act
            var changedSettings = new SettingsModel
            {
                DateTimeSettings =
                {
                    TimeFormat = string.Empty,
                    TimeZone = string.Empty,
                    LongDateFormat = string.Empty,
                    ShortDateFormat = string.Empty,
                },
                ConfirmationSettings =
                {
                    AskDownloadEpisodeConfirmation = false,
                    AskDownloadMovieConfirmation = false,
                    AskDownloadSeasonConfirmation = false,
                    AskDownloadTvShowConfirmation = false,
                },
                LanguageSettings =
                {
                    Language = string.Empty,
                },
            };
            sut.UpdateSettings(changedSettings);
            sut.Reset();

            // Assert
            sut.GetSettingsModel().ShouldBeEquivalentTo(sut.GetDefaultSettingsModel());
        }
    }
}