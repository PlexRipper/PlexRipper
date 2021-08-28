using Environment;
using FluentResults;
using Moq;
using PlexRipper.Application.Common;
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
        #region Fields

        private readonly Mock<IFileSystem> _fileSystem;

        private readonly Mock<IPathSystem> _pathSystem;

        private readonly Mock<UserSettings> _sut;

        #endregion

        public UserSettings_Setup_UnitTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.SetupLogging(output);
            _pathSystem = new Mock<IPathSystem>();
            _fileSystem = new Mock<IFileSystem>();
            _sut = new Mock<UserSettings>(MockBehavior.Strict, _pathSystem.Object, _fileSystem.Object);

            _pathSystem.Setup(x => x.ConfigFileName).Returns("Test_PlexRipperSettings.json");
            _pathSystem.Setup(x => x.ConfigDirectory).Returns("/config");
        }

        [Fact]
        public void UserSettings_Setup_ShouldHaveSuccessfulSetup_WhenSettingsAreLoaded()
        {
            // Arrange
            _fileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns(Result.Ok(true));
            _fileSystem.Setup(x => x.FileReadAllText(It.IsAny<string>())).Returns(Result.Ok(UserSettingsFakeData.JsonSettings));

            // Act
            var setupResult = _sut.Object.Setup();

            // Assert
            setupResult.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void UserSettings_Setup_ShouldReturnFailedResult_WhenFileExistsFailed()
        {
            // Arrange
            _fileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns(Result.Fail(""));

            // Act
            var setupResult = _sut.Object.Setup();

            // Assert
            setupResult.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public void UserSettings_Setup_ShouldCreateSettingsFile_WhenSettingsFileDoesntExist()
        {
            // Arrange
            _fileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns(Result.Ok(false));
            _fileSystem.Setup(x => x.FileWriteAllText(It.IsAny<string>(), It.IsAny<string>())).Returns(Result.Ok());

            // Act
            var setupResult = _sut.Object.Setup();

            // Assert
            setupResult.IsSuccess.ShouldBeTrue();
            _fileSystem.Verify(x => x.FileWriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void UserSettings_Setup_ShouldLoadSettingsFile_WhenSettingsFileExists()
        {
            // Arrange
            _fileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns(Result.Ok(true));
            _fileSystem.Setup(x => x.FileReadAllText(It.IsAny<string>())).Returns(Result.Ok(UserSettingsFakeData.JsonSettings));

            // Act
            var setupResult = _sut.Object.Setup();

            // Assert
            setupResult.IsSuccess.ShouldBeTrue();
        }
    }
}