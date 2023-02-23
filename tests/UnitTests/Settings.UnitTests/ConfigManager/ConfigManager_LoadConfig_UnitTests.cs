using System.Text.Json;
using Autofac;
using Environment;
using FileSystem.Contracts;
using Logging.Interface;
using PlexRipper.Application;
using PlexRipper.Settings;
using Settings.Contracts;

namespace Settings.UnitTests;

public class ConfigManager_LoadConfig_UnitTests : BaseUnitTest<ConfigManager>
{
    public ConfigManager_LoadConfig_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void ShouldLoadSettingsAndSendToUserSettings_WhenSettingsCanBeReadFromFile()
    {
        // Arrange
        var settingsModel = FakeData.GetSettingsModelJson();
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns(() => "TEST_PlexRipperSettings.json");
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigDirectory).Returns(() => "");
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(() => "");
        mock.Mock<IFileSystem>().Setup(x => x.FileReadAllText(It.IsAny<string>())).Returns(() => Result.Ok(settingsModel));
        mock.Mock<IUserSettings>().Setup(x => x.SetFromJsonObject(It.IsAny<JsonElement>())).Returns(Result.Ok);

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
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns(() => "TEST_PlexRipperSettings.json");
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(() => "");
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigDirectory).Returns(() => "");
        mock.Mock<IFileSystem>().Setup(x => x.FileReadAllText(It.IsAny<string>())).Returns(() => Result.Fail(""));
        mock.Mock<IUserSettings>().Setup(x => x.Reset());

        // Were mocking other methods from ConfigManager, that's why we need to mock it manually here
        var sut = new Mock<ConfigManager>(
            MockBehavior.Strict,
            mock.Container.Resolve<ILog>(),
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
        mock.Mock<IFileSystem>().Setup(x => x.FileReadAllText(It.IsAny<string>())).Returns(() => Result.Ok("{}"));
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns(() => "TEST_PlexRipperSettings.json");
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(() => "/");
        mock.Mock<IUserSettings>().Setup(x => x.SetFromJsonObject(It.IsAny<JsonElement>())).Returns(Result.Fail(""));
        mock.Mock<IUserSettings>().Setup(x => x.Reset());

        // Were mocking other methods from ConfigManager, that's why we need to mock it manually here
        var sut = new Mock<ConfigManager>(
            MockBehavior.Strict,
            mock.Container.Resolve<ILog>(),
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
        mock.Mock<IFileSystem>().Setup(x => x.FileReadAllText(It.IsAny<string>())).Returns(() => Result.Ok("@#$%^&"));
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns(() => "TEST_PlexRipperSettings.json");
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(() => "");
        mock.Mock<IUserSettings>().Setup(x => x.Reset());

        // Were mocking other methods from ConfigManager, that's why we need to mock it manually here
        var sut = new Mock<ConfigManager>(
            MockBehavior.Strict,
            mock.Container.Resolve<ILog>(),
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