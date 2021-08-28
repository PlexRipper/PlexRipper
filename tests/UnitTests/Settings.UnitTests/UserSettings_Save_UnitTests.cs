using Environment;
using FluentResults;
using Logging;
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
    public class UserSettings_Save_UnitTests
    {
        #region Fields

        private readonly Mock<IFileSystem> _fileSystem;

        private readonly Mock<IPathSystem> _pathSystem;

        private readonly UserSettings _sut;

        #endregion

        public UserSettings_Save_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
            _pathSystem = new Mock<IPathSystem>();
            _fileSystem = new Mock<IFileSystem>();
            _sut = new Mock<UserSettings>(MockBehavior.Strict, _pathSystem.Object, _fileSystem.Object).Object;

            _pathSystem.Setup(x => x.ConfigFileName).Returns("Test_PlexRipperSettings.json");
            _pathSystem.Setup(x => x.ConfigDirectory).Returns("/config");
        }

        [Fact]
        public void UserSettings_Save_ShouldSaveSettings_WhenSettingChanged()
        {
            // Arrange
            _fileSystem.Setup(x => x.FileReadAllText(It.IsAny<string>())).Returns(Result.Ok(UserSettingsFakeData.JsonSettings));
            _fileSystem.Setup(x => x.FileWriteAllText(It.IsAny<string>(), It.IsAny<string>())).Returns(Result.Ok());

            // Act
            var loadResult = _sut.Load();
            _sut.ActiveAccountId = 9000;
            _sut.DownloadSegments = 9000;
            var saveResult = _sut.Save();

            // Assert
            loadResult.IsSuccess.ShouldBeTrue();
            saveResult.IsSuccess.ShouldBeTrue();
            _sut.ActiveAccountId.ShouldBe(9000);
            _sut.DownloadSegments.ShouldBe(9000);
        }

        [Fact]
        public void UserSettings_Save_ShouldHaveFailedResult_WhenFileFailedToBeWritten()
        {
            // Arrange
            _fileSystem.Setup(x => x.FileWriteAllText(It.IsAny<string>(), It.IsAny<string>())).Returns(Result.Fail("Failed to write save settings"));

            // Act
            var saveResult = _sut.Save();

            // Assert
            saveResult.IsSuccess.ShouldBeFalse();
        }
    }
}