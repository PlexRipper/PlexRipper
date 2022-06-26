using PlexRipper.Settings.Modules;

namespace Settings.UnitTests.Modules
{
    public class DisplaySettingsModule_Update_UnitTests
    {
        public DisplaySettingsModule_Update_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public void ShouldUpdateSettingsModule_WhenGivenValidSettingsObject()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DisplaySettingsModule>();
            var settings = FakeData.GetDisplaySettings().Generate();

            // Act
            var updateResult = _sut.Update(settings);

            // Assert
            updateResult.TvShowViewMode.ShouldBe(settings.TvShowViewMode);
            updateResult.MovieViewMode.ShouldBe(settings.MovieViewMode);
        }
    }
}