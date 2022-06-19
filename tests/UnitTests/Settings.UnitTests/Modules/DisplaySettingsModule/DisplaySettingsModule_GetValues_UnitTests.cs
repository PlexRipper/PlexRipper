using Autofac.Extras.Moq;
using Logging;
using PlexRipper.Application;
using PlexRipper.BaseTests;
using PlexRipper.Settings.Modules;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Settings.UnitTests.Modules
{
    public class DisplaySettingsModule_GetValues_UnitTests
    {
        public DisplaySettingsModule_GetValues_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public void ShouldReturnCurrentValues_WhenValidSettingsValuesAreSet()
        {
            // Arrange
            var displaySettingsModule = new DisplaySettingsModule();

            // Act
            IDisplaySettings getValuesResult = displaySettingsModule.GetValues();

            // Assert
            getValuesResult.ShouldNotBeNull();
            getValuesResult.TvShowViewMode.ShouldBe(displaySettingsModule.TvShowViewMode);
            getValuesResult.MovieViewMode.ShouldBe(displaySettingsModule.MovieViewMode);
        }
    }
}