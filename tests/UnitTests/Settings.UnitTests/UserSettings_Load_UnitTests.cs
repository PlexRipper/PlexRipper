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
    public class UserSettings_Load_UnitTests
    {
        #region Fields

        private readonly Mock<IFileSystem> _fileSystem;

        private readonly Mock<IPathSystem> _pathSystem;

        private readonly UserSettings _sut;

        #endregion

        public UserSettings_Load_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
            _pathSystem = new Mock<IPathSystem>();
            _fileSystem = new Mock<IFileSystem>();
            _sut = new Mock<UserSettings>(MockBehavior.Strict, _pathSystem.Object, _fileSystem.Object).Object;

            _pathSystem.Setup(x => x.ConfigFileName).Returns("Test_PlexRipperSettings.json");
            _pathSystem.Setup(x => x.ConfigDirectory).Returns("/config");
        }

        [Fact]
        public void UserSettings_Load_ShouldLoadSettings_WhenGivenValidSettings()
        {
            // Arrange
            _fileSystem.Setup(x => x.FileReadAllText(It.IsAny<string>())).Returns(Result.Ok(UserSettingsFakeData.JsonSettings));

            // Act
            var loadResult = _sut.Load();

            // Assert
            loadResult.IsSuccess.ShouldBeTrue();
            _sut.ActiveAccountId.ShouldBe(999);
            _sut.DownloadSegments.ShouldBe(40);
        }

        [Fact]
        public void UserSettings_Load_ShouldFailToLoadSettings_WhenFailingToReadSettings()
        {
            // Arrange
            _fileSystem.Setup(x => x.FileReadAllText(It.IsAny<string>())).Returns(Result.Fail("Test Fail"));

            // Act
            var loadResult = _sut.Load();

            // Assert
            loadResult.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public void UserSettings_Load_ShouldThrowExceptionWhenFailToSerializeSettings_WhenReadingInvalidJsonSettings()
        {
            // Arrange
            _fileSystem.Setup(x => x.FileReadAllText(It.IsAny<string>())).Returns(Result.Ok("Invalid Json"));

            // Act
            var loadResult = _sut.Load();

            // Assert
            loadResult.IsFailed.ShouldBeTrue();
            loadResult.HasException().ShouldBeTrue();
        }
    }
}