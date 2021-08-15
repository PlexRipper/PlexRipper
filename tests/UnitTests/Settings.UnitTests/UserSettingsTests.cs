using Moq;
using PlexRipper.BaseTests;
using PlexRipper.Settings;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Settings.UnitTests
{
    public class UserSettingsTests
    {
        private readonly Mock<UserSettings> _sut;

        public UserSettingsTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.SetupLogging(output);
            _sut = new Mock<UserSettings>(MockBehavior.Strict);
        }

        [Fact]
        public void UserSettings_ShouldHaveChangedProperty_WhenPropertyIsSaved()
        {
            // Arrange
            var userSettings = _sut.Object;

            // Act
            userSettings.Setup();
            userSettings.DownloadSegments = 999;
            userSettings.ActiveAccountId = 999;
            userSettings.Save();

            // Assert
            userSettings.DownloadSegments.ShouldBe(999);
            userSettings.ActiveAccountId.ShouldBe(999);
        }
    }
}