using System.Reactive.Subjects;
using Autofac;
using Environment;
using PlexRipper.Application;
using PlexRipper.Settings;

namespace Settings.UnitTests;

public class ConfigManager_Setup_UnitTests : BaseUnitTest<ConfigManager>
{
    public ConfigManager_Setup_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void ShouldLoadConfigDuringSetup_WhenConfigFileAlreadyExists()
    {
        // Arrange
        mock.Mock<IUserSettings>().SetupGet(x => x.SettingsUpdated).Returns(new Subject<ISettingsModel>());
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns(() => "TEST_PlexRipperSettings.json");
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(() => "/");
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigDirectory).Returns(() => "/TEST_PlexRipperSettings.json");
        mock.Mock<IDirectorySystem>().Setup(x => x.Exists(It.IsAny<string>())).Returns(Result.Ok(true));

        // Were mocking other methods from ConfigManager, that's why we need to mock it manually here
        var sut = new Mock<ConfigManager>(
            MockBehavior.Strict,
            mock.Container.Resolve<IFileSystem>(),
            mock.Container.Resolve<IDirectorySystem>(),
            mock.Container.Resolve<IPathProvider>(),
            mock.Container.Resolve<IUserSettings>());
        sut.Setup(x => x.ConfigFileExists()).Returns(true);
        sut.Setup(x => x.LoadConfig()).Returns(Result.Ok);

        // Act
        var resetResult = sut.Object.Setup();

        // Assert
        resetResult.IsSuccess.ShouldBeTrue();
        sut.Verify(x => x.LoadConfig(), Times.Once);
        mock.Mock<IUserSettings>().VerifyGet(x => x.SettingsUpdated, Times.Once);
    }

    [Fact]
    public void ShouldCreateConfigFile_WhenConfigFileDoesNotExists()
    {
        // Arrange
        var settingsModel = FakeData.GetSettingsModel().Generate();
        mock.Mock<IUserSettings>().SetupGet(x => x.SettingsUpdated).Returns(new Subject<ISettingsModel>());
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns(() => "TEST_PlexRipperSettings.json");
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigDirectory).Returns(() => "/");
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(() => "/TEST_PlexRipperSettings.json");
        mock.Mock<IUserSettings>().Setup(x => x.GetSettingsModel()).Returns(settingsModel);
        mock.Mock<IFileSystem>().Setup(x => x.FileWriteAllText(It.IsAny<string>(), It.IsAny<string>())).Returns(Result.Ok);
        mock.Mock<IDirectorySystem>().Setup(x => x.Exists(It.IsAny<string>())).Returns(Result.Ok(false));
        mock.Mock<IDirectorySystem>().Setup(x => x.CreateDirectory(It.IsAny<string>())).Returns(Result.Ok());

        // Were mocking other methods from ConfigManager, that's why we need to mock it manually here
        var sut = new Mock<ConfigManager>(
            MockBehavior.Strict,
            mock.Container.Resolve<IFileSystem>(),
            mock.Container.Resolve<IDirectorySystem>(),
            mock.Container.Resolve<IPathProvider>(),
            mock.Container.Resolve<IUserSettings>());
        sut.Setup(x => x.ConfigFileExists()).Returns(false);
        sut.Setup(x => x.SaveConfig()).Returns(Result.Ok);

        // Act
        var resetResult = sut.Object.Setup();

        // Assert
        resetResult.IsSuccess.ShouldBeTrue();
        sut.Verify(x => x.LoadConfig(), Times.Never);
        sut.Verify(x => x.ConfigFileExists(), Times.Once);
        mock.Mock<IUserSettings>().VerifyGet(x => x.SettingsUpdated, Times.Once);
    }
}