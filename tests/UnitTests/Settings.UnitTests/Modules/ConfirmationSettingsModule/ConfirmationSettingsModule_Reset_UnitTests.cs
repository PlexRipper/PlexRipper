using Autofac.Extras.Moq;
using Logging;
using PlexRipper.Settings.Models;
using PlexRipper.Settings.Modules;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Settings.UnitTests.Modules
{
    public class ConfirmationSettingsModule_Reset_UnitTests
    {
        public ConfirmationSettingsModule_Reset_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public void ShouldUpdateAndThenResetSettingsModule_WhenCallingResetAfterUpdate()
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
            var resetResult = _sut.Reset();

            // Assert
            updateResult.AskDownloadMovieConfirmation.ShouldBeFalse();
            updateResult.AskDownloadTvShowConfirmation.ShouldBeFalse();
            updateResult.AskDownloadSeasonConfirmation.ShouldBeFalse();
            updateResult.AskDownloadEpisodeConfirmation.ShouldBeFalse();

            resetResult.AskDownloadMovieConfirmation.ShouldBeTrue();
            resetResult.AskDownloadTvShowConfirmation.ShouldBeTrue();
            resetResult.AskDownloadSeasonConfirmation.ShouldBeTrue();
            resetResult.AskDownloadEpisodeConfirmation.ShouldBeTrue();
        }
    }
}