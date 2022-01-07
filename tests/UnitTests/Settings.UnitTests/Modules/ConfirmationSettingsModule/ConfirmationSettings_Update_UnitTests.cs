using Autofac.Extras.Moq;
using Logging;
using PlexRipper.Settings.Models;
using PlexRipper.Settings.Modules;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Settings.UnitTests.Modules
{
    public class ConfirmationSettings_Update_UnitTests
    {
        public ConfirmationSettings_Update_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public void ShouldHaveNoUpdates_WhenGivenAnEmptyList()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<ConfirmationSettingsModule>();
            var settings = new ConfirmationSettings
            {
                AskDownloadMovieConfirmation = false,
                AskDownloadEpisodeConfirmation = false,
                AskDownloadSeasonConfirmation = false,
                AskDownloadTvShowConfirmation = false,
            };

            // Act
            var updateResult = _sut.Update(settings);

            // Assert
            updateResult.IsSuccess.ShouldBeTrue();
        }
    }
}