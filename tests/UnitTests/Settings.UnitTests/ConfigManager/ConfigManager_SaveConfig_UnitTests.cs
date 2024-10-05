using System.Reactive.Subjects;
using Autofac;
using Environment;
using FileSystem.Contracts;
using Logging.Interface;
using PlexRipper.Settings;
using Settings.Contracts;

namespace Settings.UnitTests;

public class ConfigManager_SaveConfig_UnitTests : BaseUnitTest<ConfigManager>
{
    public ConfigManager_SaveConfig_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldLoadConfigDuringSetup_WhenConfigFileAlreadyExists()
    {
        // Arrange
        mock.Mock<IUserSettings>().SetupGet(x => x.SettingsUpdated).Returns(new Subject<UserSettings>());
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns(() => "TEST_PlexRipperSettings.json");
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(() => "/");
        mock.Mock<IFileSystem>()
            .Setup(x => x.FileWriteAllText(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Result.Ok);

        // Were mocking other methods from ConfigManager, that's why we need to mock it manually here
        var sut = new Mock<ConfigManager>(
            MockBehavior.Strict,
            mock.Container.Resolve<ILog>(),
            mock.Container.Resolve<IFileSystem>(),
            mock.Container.Resolve<IDirectorySystem>(),
            mock.Container.Resolve<IPathProvider>(),
            mock.Container.Resolve<IUserSettings>()
        );
        sut.Setup(x => x.SaveConfig()).CallBase();
        sut.Setup(x => x.ConfigFileExists()).Returns(true);
        sut.Setup(x => x.LoadConfig()).Returns(Result.Ok);

        // Act
        var resetResult = sut.Object.SaveConfig();

        // Assert
        resetResult.IsSuccess.ShouldBeTrue();
        mock.Mock<IFileSystem>().Verify(x => x.FileWriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
}
