using PlexRipper.Settings.Modules;

namespace Settings.UnitTests.Modules;

public class DisplaySettingsModule_Update_UnitTests : BaseUnitTest<DisplaySettingsModule>
{
    public DisplaySettingsModule_Update_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void ShouldUpdateSettingsModule_WhenGivenValidSettingsObject()
    {
        // Arrange
        var settings = FakeData.GetDisplaySettings().Generate();

        // Act
        var updateResult = _sut.Update(settings);

        // Assert
        updateResult.TvShowViewMode.ShouldBe(settings.TvShowViewMode);
        updateResult.MovieViewMode.ShouldBe(settings.MovieViewMode);
    }
}