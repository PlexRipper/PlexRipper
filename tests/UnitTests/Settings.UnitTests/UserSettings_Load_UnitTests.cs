using Autofac.Extras.Moq;
using Environment;
using FluentResults;
using Logging;
using Moq;
using PlexRipper.Application;
using PlexRipper.Settings;
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
        public void UserSettings_Load_ShouldFailToLoadSettings_WhenFailingToReadSettings()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            mock.Mock<IFileSystem>().Setup(x => x.FileReadAllText(It.IsAny<string>()))
                .Returns(Result.Fail("Test Fail"));
            mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns("/config/Test_PlexRipperSettings.json");

            var _sut = mock.Create<IConfigManager>();

            // Act
            var loadResult = _sut.LoadConfig();

            // Assert
            loadResult.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public void UserSettings_Load_ShouldThrowExceptionWhenFailToSerializeSettings_WhenReadingInvalidJsonSettings()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns("/config/Test_PlexRipperSettings.json");
            mock.Mock<IFileSystem>().Setup(x => x.FileReadAllText(It.IsAny<string>()))
                .Returns(Result.Ok());
            mock.Mock<IFileSystem>().Setup(x => x.FileReadAllText(It.IsAny<string>())).Returns(Result.Ok("Invalid Json"));
            mock.Mock<IFileSystem>().Setup(x => x.FileWriteAllText(It.IsAny<string>(), It.IsAny<string>())).Returns(Result.Ok());
            var _sut = mock.Create<IConfigManager>();

            // Act
            var loadResult = _sut.LoadConfig();

            // Assert
            loadResult.IsFailed.ShouldBeTrue();
            loadResult.HasException().ShouldBeTrue();
        }
    }
}