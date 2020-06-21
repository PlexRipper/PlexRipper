using PlexRipper.Settings;
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
            // Assert

        }
    }
}
