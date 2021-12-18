using Autofac.Extras.Moq;
using Environment;
using FluentResults;
using Logging;
using Moq;
using PlexRipper.Application;
using PlexRipper.Settings;
using Settings.UnitTests.MockData;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Settings.UnitTests
{
    public class UserSettings_Load_UnitTests
    {
        public UserSettings_Load_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public void UserSettings_Load_ShouldLoadSettings_WhenGivenValidSettings()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            mock.Mock<IFileSystem>().Setup(x => x.FileReadAllText(It.IsAny<string>()))
                .Returns(Result.Ok(UserSettingsFakeData.GetValidJsonSettings()));
            mock.Mock<IFileSystem>().Setup(x => x.FileWriteAllText(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Result.Ok());
            mock.Mock<IPathSystem>().SetupGet(x => x.ConfigFileName).Returns("Test_PlexRipperSettings.json");
            mock.Mock<IPathSystem>().SetupGet(x => x.ConfigDirectory).Returns("/config");
            mock.Mock<IPathSystem>().SetupGet(x => x.ConfigFileLocation).Returns("/config/Test_PlexRipperSettings.json");

            var _sut = mock.Create<UserSettings>();

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
            using var mock = AutoMock.GetStrict();
            mock.Mock<IFileSystem>().Setup(x => x.FileReadAllText(It.IsAny<string>()))
                .Returns(Result.Fail("Test Fail"));
            mock.Mock<IPathSystem>().SetupGet(x => x.ConfigFileLocation).Returns("/config/Test_PlexRipperSettings.json");

            var _sut = mock.Create<UserSettings>();

            // Act
            var loadResult = _sut.Load();

            // Assert
            loadResult.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public void UserSettings_Load_ShouldThrowExceptionWhenFailToSerializeSettings_WhenReadingInvalidJsonSettings()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            mock.Mock<IPathSystem>().SetupGet(x => x.ConfigFileLocation).Returns("/config/Test_PlexRipperSettings.json");
            mock.Mock<IFileSystem>().Setup(x => x.FileReadAllText(It.IsAny<string>()))
                .Returns(Result.Ok(UserSettingsFakeData.GetValidJsonSettings()));
            mock.Mock<IFileSystem>().Setup(x => x.FileReadAllText(It.IsAny<string>())).Returns(Result.Ok("Invalid Json"));
            mock.Mock<IFileSystem>().Setup(x => x.FileWriteAllText(It.IsAny<string>(), It.IsAny<string>())).Returns(Result.Ok());
            var _sut = mock.Create<UserSettings>();

            // Act
            var loadResult = _sut.Load();

            // Assert
            loadResult.IsFailed.ShouldBeTrue();
            loadResult.HasException().ShouldBeTrue();
        }
    }
}