using PlexRipper.Settings.Models;
using PlexRipper.Settings.Modules;

namespace Settings.UnitTests.Modules
{
    public class ConfirmationSettingsModule_Update_UnitTests
    {
        public ConfirmationSettingsModule_Update_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public void ShouldUpdateSettingsModule_WhenGivenValidSettingsObject()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<ConfirmationSettingsModule>();
            var settings = new ConfirmationSettings
            {
                AskDownloadMovieConfirmation = false,
                AskDownloadTvShowConfirmation = false,
                AskDownloadSeasonConfirmation = false,
                AskDownloadEpisodeConfirmation = false,
            };

            // Act
            var updateResult = _sut.Update(settings);

            // Assert
            updateResult.AskDownloadMovieConfirmation.ShouldBeFalse();
            updateResult.AskDownloadTvShowConfirmation.ShouldBeFalse();
            updateResult.AskDownloadSeasonConfirmation.ShouldBeFalse();
            updateResult.AskDownloadEpisodeConfirmation.ShouldBeFalse();
        }
    }
}