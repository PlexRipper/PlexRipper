using Autofac.Extras.Moq;
using Environment;
using FluentResults;
using Logging;
using Moq;
using PlexRipper.Application;
using PlexRipper.BaseTests;
using PlexRipper.Settings;
using Settings.UnitTests.MockData;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Settings.UnitTests
{
    public class UserSettings_Setup_UnitTests
    {
        public UserSettings_Setup_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public void UserSettings_Setup_ShouldHaveSuccessfulSetup_WhenSettingsAreLoaded()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            mock.Mock<IFileSystem>().Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);
            mock.Mock<IFileSystem>().Setup(x => x.FileWriteAllText(It.IsAny<string>(), It.IsAny<string>())).Returns(Result.Ok());
            mock.Mock<IFileSystem>().Setup(x => x.FileReadAllText(It.IsAny<string>())).Returns(Result.Ok(UserSettingsFakeData.GetValidJsonSettings()));
            mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns("/config/Test_PlexRipperSettings.json");

            // Act
            var setupResult = mock.Create<UserSettings>().Setup();

            // Assert
            setupResult.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void UserSettings_Setup_ShouldReturnFailedResult_WhenFileExistsFailed()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            mock.Mock<IFileSystem>().Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);
            var _sut = mock.Create<UserSettings>();

            // Act
            var setupResult = _sut.Setup();

            // Assert
            setupResult.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public void UserSettings_Setup_ShouldCreateSettingsFile_WhenSettingsFileDoesntExist()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            mock.Mock<IFileSystem>().Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);
            mock.Mock<IFileSystem>().Setup(x => x.FileWriteAllText(It.IsAny<string>(), It.IsAny<string>())).Returns(Result.Ok());
            var _sut = mock.Create<UserSettings>();

            // Act
            var setupResult = _sut.Setup();

            // Assert
            setupResult.IsSuccess.ShouldBeTrue();
            mock.Mock<IFileSystem>().Verify(x => x.FileWriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void UserSettings_Setup_ShouldLoadSettingsFile_WhenSettingsFileExists()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns("/config/Test_PlexRipperSettings.json");
            mock.Mock<IFileSystem>().Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);
            mock.Mock<IFileSystem>().Setup(x => x.FileReadAllText(It.IsAny<string>())).Returns(Result.Ok(UserSettingsFakeData.GetValidJsonSettings()));
            mock.Mock<IFileSystem>().Setup(x => x.FileWriteAllText(It.IsAny<string>(), It.IsAny<string>())).Returns(Result.Ok());

            var _sut = mock.Create<UserSettings>();

            // Act
            var setupResult = _sut.Setup();

            // Assert
            setupResult.IsSuccess.ShouldBeTrue();
        }
    }
}