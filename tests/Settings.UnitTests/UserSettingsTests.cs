using PlexRipper.Settings;
using Shouldly;
using Xunit;

namespace Settings.UnitTests
{
    public class UserSettingsTests
    {
        [Fact]
        public void SaveSettingsToJsonFile()
        {
            // Arrange
            var settings = new UserSettings();

            // Act
            settings.Save();
            settings.Settings.ApiKey = "TEST!@#";
            settings.Settings.ApiKey = "TEST123";
            settings.Load();

            // Assert
            settings.Settings.ApiKey.ShouldBe("TEST123");
        }
    }
}
