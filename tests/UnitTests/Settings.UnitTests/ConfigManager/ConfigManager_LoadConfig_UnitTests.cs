using System.Text.Json;
using Autofac;
using Autofac.Extras.Moq;
using Environment;
using FluentResults;
using Logging;
using Moq;
using PlexRipper.Application;
using PlexRipper.BaseTests;
using PlexRipper.Settings;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Settings.UnitTests
{
    public class ConfigManager_LoadConfig_UnitTests
    {
        public ConfigManager_LoadConfig_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public void ShouldLoadSettingsAndSendToUserSettings_WhenSettingsCanBeReadFromFile()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var settingsModel = FakeData.GetSettingsModelJson();
            mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns(() => "TEST_PlexRipperSettings.json");
            mock.Mock<IPathProvider>().SetupGet(x => x.ConfigDirectory).Returns(() => "");
            mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(() => "");
            mock.Mock<IFileSystem>().Setup(x => x.FileReadAllText(It.IsAny<string>())).Returns(() => Result.Ok(settingsModel));
            mock.Mock<IUserSettings>().Setup(x => x.SetFromJsonObject(It.IsAny<JsonElement>())).Returns(Result.Ok);
            var _sut = mock.Create<ConfigManager>();

            // Act
            var loadResult = _sut.LoadConfig();

            // Assert
            loadResult.IsSuccess.ShouldBeTrue();
            mock.Mock<IUserSettings>().Verify(x => x.Reset(), Times.Never);
            mock.Mock<IUserSettings>().Verify(x => x.SetFromJsonObject(It.IsAny<JsonElement>()), Times.Once);
        }

        [Fact]
        public void ShouldResetSettings_WhenFailingToReadSettingsFromFile()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns(() => "TEST_PlexRipperSettings.json");
            mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(() => "");
            mock.Mock<IPathProvider>().SetupGet(x => x.ConfigDirectory).Returns(() => "");
            mock.Mock<IFileSystem>().Setup(x => x.FileReadAllText(It.IsAny<string>())).Returns(() => Result.Fail(""));
            mock.Mock<IUserSettings>().Setup(x => x.Reset());

            var sut = new Mock<ConfigManager>(
                MockBehavior.Strict,
                mock.Container.Resolve<IFileSystem>(),
                mock.Container.Resolve<IDirectorySystem>(),
                mock.Container.Resolve<IPathProvider>(),
                mock.Container.Resolve<IUserSettings>());
            sut.Setup(x => x.ResetConfig()).Returns(Result.Ok);
            sut.Setup(x => x.LoadConfig()).CallBase();

            // Act
            var loadResult = sut.Object.LoadConfig();

            // Assert
            loadResult.IsSuccess.ShouldBeTrue();
            sut.Verify(x => x.ResetConfig(), Times.Once);
        }

        [Fact]
        public void ShouldResetSettingsWhenUserSettingsCouldNotBeSetFromJsonSerialization_WhenReadingInvalidParsedJsonSettings()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            mock.Mock<IFileSystem>().Setup(x => x.FileReadAllText(It.IsAny<string>())).Returns(() => Result.Ok("{}"));
            mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns(() => "TEST_PlexRipperSettings.json");
            mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(() => "/");
            mock.Mock<IUserSettings>().Setup(x => x.SetFromJsonObject(It.IsAny<JsonElement>())).Returns(Result.Fail(""));
            mock.Mock<IUserSettings>().Setup(x => x.Reset());

            var sut = new Mock<ConfigManager>(
                MockBehavior.Strict,
                mock.Container.Resolve<IFileSystem>(),
                mock.Container.Resolve<IDirectorySystem>(),
                mock.Container.Resolve<IPathProvider>(),
                mock.Container.Resolve<IUserSettings>());
            sut.Setup(x => x.ResetConfig()).Returns(Result.Ok);
            sut.Setup(x => x.LoadConfig()).CallBase();

            // Act
            var loadResult = sut.Object.LoadConfig();

            // Assert
            loadResult.IsSuccess.ShouldBeTrue();
            sut.Verify(x => x.ResetConfig(), Times.Once);

        }

        [Fact]
        public void ShouldResetSettingsWhenSerializationThrowsException_WhenReadingInvalidJsonSettings()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            mock.Mock<IFileSystem>().Setup(x => x.FileReadAllText(It.IsAny<string>())).Returns(() => Result.Ok("@#$%^&"));
            mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns(() => "TEST_PlexRipperSettings.json");
            mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(() => "");
            mock.Mock<IUserSettings>().Setup(x => x.Reset());

            var sut = new Mock<ConfigManager>(
                MockBehavior.Strict,
                mock.Container.Resolve<IFileSystem>(),
                mock.Container.Resolve<IDirectorySystem>(),
                mock.Container.Resolve<IPathProvider>(),
                mock.Container.Resolve<IUserSettings>());
            sut.Setup(x => x.ResetConfig()).Returns(Result.Ok);
            sut.Setup(x => x.LoadConfig()).CallBase();

            // Act
            var loadResult = sut.Object.LoadConfig();

            // Assert
            loadResult.IsSuccess.ShouldBeTrue();
            sut.Verify(x => x.ResetConfig(), Times.Once);

        }
    }
}