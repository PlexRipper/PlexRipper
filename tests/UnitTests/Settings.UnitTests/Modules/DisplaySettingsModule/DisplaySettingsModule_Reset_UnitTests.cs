using Autofac.Extras.Moq;
using PlexRipper.Settings.Models;
using PlexRipper.Settings.Modules;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Settings.UnitTests.Modules
{
    public class DisplaySettingsModule_Reset_UnitTests
    {
        public DisplaySettingsModule_Reset_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public void ShouldUpdateAndThenResetSettingsModule_WhenCallingResetAfterUpdate()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DisplaySettingsModule>();
            var settings = new DisplaySettings
            {
                MovieViewMode = ViewMode.Table,
                TvShowViewMode = ViewMode.Table,
            };

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
}