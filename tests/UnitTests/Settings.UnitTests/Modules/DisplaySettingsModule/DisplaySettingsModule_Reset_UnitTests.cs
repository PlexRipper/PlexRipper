using PlexRipper.Settings;

namespace Settings.UnitTests.Modules;

public class DisplaySettingsModule_Reset_UnitTests : BaseUnitTest<DisplaySettingsModule>
{
    public DisplaySettingsModule_Reset_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldUpdateAndThenResetSettingsModule_WhenCallingResetAfterUpdate()
    {
        // Arrange
        var settings = new DisplaySettings { MovieViewMode = ViewMode.Table, TvShowViewMode = ViewMode.Table };

        // Act
        var updateResult = _sut.Update(settings);
        var resetResult = _sut.Reset();

        // Assert
        updateResult.MovieViewMode.ShouldBe(ViewMode.Table);
        updateResult.TvShowViewMode.ShouldBe(ViewMode.Table);

        resetResult.MovieViewMode.ShouldBe(_sut.DefaultValues().MovieViewMode);
        resetResult.TvShowViewMode.ShouldBe(_sut.DefaultValues().TvShowViewMode);
    }
}
