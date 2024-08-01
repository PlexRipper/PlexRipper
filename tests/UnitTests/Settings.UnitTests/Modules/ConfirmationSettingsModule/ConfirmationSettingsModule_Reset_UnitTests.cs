using PlexRipper.Settings;

namespace Settings.UnitTests.Modules;

public class ConfirmationSettingsModule_Reset_UnitTests : BaseUnitTest<ConfirmationSettingsModule>
{
    public ConfirmationSettingsModule_Reset_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldUpdateAndThenResetSettingsModule_WhenCallingResetAfterUpdate()
    {
        // Arrange
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
