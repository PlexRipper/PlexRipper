using System.IO.Abstractions;
using System.Reactive.Subjects;
using Reaparr.Environment;
using Reaparr.Settings.Contracts;

namespace Reaparr.Settings.UnitTests;

public class ConfigManagerMigrateLegacyFileNamesUnitTests : BaseUnitTest<ConfigManager>
{
    public ConfigManagerMigrateLegacyFileNamesUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldRenameLegacyConfigFile_WhenOldExistsAndNewMissing()
    {
        // Arrange
        Mock.Mock<IUserSettings>().SetupGet(x => x.SettingsUpdated).Returns(new Subject<UserSettings>());
        var configDir = "/config";
        var oldConfigPath = "/config/PlexRipperSettings.json";
        var newConfigPath = "/config/TEST_ReaparrSettings.json";
        var oldDbPath = "/config/PlexRipperDB.db";
        var newDbPath = "/config/ReaparrDB.db";

        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigDirectory).Returns(configDir);
        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns("TEST_ReaparrSettings.json");
        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(newConfigPath);

        Mock.Mock<IDirectory>().Setup(x => x.Exists(configDir)).Returns(true);
        Mock.Mock<IPath>().Setup(x => x.Combine(configDir, "PlexRipperSettings.json")).Returns(oldConfigPath);
        Mock.Mock<IPath>().Setup(x => x.Combine(configDir, "PlexRipperDB.db")).Returns(oldDbPath);

        Mock.Mock<IFile>().Setup(x => x.Exists(oldConfigPath)).Returns(true).Verifiable(Times.Once);
        Mock.Mock<IFile>()
            .SetupSequence(x => x.Exists(newConfigPath))
            .Returns(false) // During migration check
            .Returns(true); // During ConfigFileExists at end of Setup

        Mock.Mock<IFile>().Setup(x => x.Move(oldConfigPath, newConfigPath)).Verifiable(Times.Once);

        // Additional setups to satisfy strict mocks during migration and load
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(newDbPath);
        Mock.Mock<IFile>().Setup(x => x.Exists(oldDbPath)).Returns(false);
        Mock.Mock<IFile>().Setup(x => x.Exists(oldDbPath + "-wal")).Returns(false);
        Mock.Mock<IFile>().Setup(x => x.Exists(oldDbPath + "-shm")).Returns(false);
        Mock.Mock<IFile>().Setup(x => x.ReadAllText(newConfigPath)).Returns("{}");
        Mock.Mock<IUserSettings>().Setup(x => x.UpdateSettings(It.IsAny<ISettingsModel>())).Returns(new UserSettings());

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IFile>().Verify(x => x.Move(oldConfigPath, newConfigPath), Times.Once);
    }

    [Fact]
    public void ShouldNotThrow_WhenNoLegacyFilesPresent()
    {
        // Arrange
        Mock.Mock<IUserSettings>().SetupGet(x => x.SettingsUpdated).Returns(new Subject<UserSettings>());
        var configDir = "/config";
        var newConfigPath = "/config/TEST_ReaparrSettings.json";
        var oldConfigPath = "/config/PlexRipperSettings.json";
        var oldDbPath = "/config/PlexRipperDB.db";
        var newDbPath = "/config/ReaparrDB.db";

        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigDirectory).Returns(configDir);
        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns("TEST_ReaparrSettings.json");
        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(newConfigPath);
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(newDbPath);
        Mock.Mock<IDirectory>().Setup(x => x.Exists(configDir)).Returns(true);
        Mock.Mock<IPath>().Setup(x => x.Combine(configDir, "PlexRipperSettings.json")).Returns(oldConfigPath);
        Mock.Mock<IPath>().Setup(x => x.Combine(configDir, "PlexRipperDB.db")).Returns(oldDbPath);
        Mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
        Mock.Mock<IFile>().Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>()));
        Mock.Mock<IFile>().Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>()));
        Mock.Mock<IFile>().Setup(x => x.ReadAllText(newConfigPath)).Returns("{}");
        Mock.Mock<IUserSettings>().Setup(x => x.UpdateSettings(It.IsAny<ISettingsModel>())).Returns(new UserSettings());

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ShouldRenameLegacyDatabaseFiles_WhenOldDbAndSidecarsExistAndNewMissing()
    {
        // Arrange
        Mock.Mock<IUserSettings>().SetupGet(x => x.SettingsUpdated).Returns(new Subject<UserSettings>());
        var configDir = "/config";
        var oldConfigPath = "/config/PlexRipperSettings.json";
        var newConfigPathForRead = "/config/TEST_ReaparrSettings.json";
        var oldDbPath = "/config/PlexRipperDB.db";
        var newDbPath = "/config/ReaparrDB.db";
        var oldWalPath = oldDbPath + "-wal";
        var newWalPath = newDbPath + "-wal";
        var oldShmPath = oldDbPath + "-shm";
        var newShmPath = newDbPath + "-shm";

        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigDirectory).Returns(configDir);
        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns("TEST_ReaparrSettings.json");
        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(newConfigPathForRead);
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(newDbPath);

        Mock.Mock<IDirectory>().Setup(x => x.Exists(configDir)).Returns(true);
        Mock.Mock<IPath>().Setup(x => x.Combine(configDir, "PlexRipperSettings.json")).Returns(oldConfigPath);
        Mock.Mock<IPath>().Setup(x => x.Combine(configDir, "PlexRipperDB.db")).Returns(oldDbPath);

        // Old exists, new missing initially
        Mock.Mock<IFile>().Setup(x => x.Exists(oldDbPath)).Returns(true);
        Mock.Mock<IFile>().Setup(x => x.Exists(newDbPath)).Returns(false);

        // Sidecars
        Mock.Mock<IFile>().Setup(x => x.Exists(oldWalPath)).Returns(true);
        Mock.Mock<IFile>().Setup(x => x.Exists(oldShmPath)).Returns(true);
        Mock.Mock<IFile>().Setup(x => x.Exists(newWalPath)).Returns(false);
        Mock.Mock<IFile>().Setup(x => x.Exists(newShmPath)).Returns(false);

        // Config file exists check at the end of Setup should pass
        Mock.Mock<IFile>().Setup(x => x.Exists(oldConfigPath)).Returns(false);
        Mock.Mock<IFile>().Setup(x => x.Exists(newConfigPathForRead)).Returns(true);
        Mock.Mock<IFile>().Setup(x => x.ReadAllText(newConfigPathForRead)).Returns("{}");
        Mock.Mock<IUserSettings>().Setup(x => x.UpdateSettings(It.IsAny<ISettingsModel>())).Returns(new UserSettings());

        Mock.Mock<IFile>().Setup(x => x.Move(oldDbPath, newDbPath)).Verifiable(Times.Once);
        Mock.Mock<IFile>().Setup(x => x.Move(oldWalPath, newWalPath)).Verifiable(Times.Once);
        Mock.Mock<IFile>().Setup(x => x.Move(oldShmPath, newShmPath)).Verifiable(Times.Once);

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IFile>().Verify(x => x.Move(oldDbPath, newDbPath), Times.Once);
        Mock.Mock<IFile>().Verify(x => x.Move(oldWalPath, newWalPath), Times.Once);
        Mock.Mock<IFile>().Verify(x => x.Move(oldShmPath, newShmPath), Times.Once);
    }

    [Fact]
    public void ShouldNotRename_WhenNewTargetsAlreadyExist()
    {
        // Arrange
        Mock.Mock<IUserSettings>().SetupGet(x => x.SettingsUpdated).Returns(new Subject<UserSettings>());
        var configDir = "/config";
        var oldConfigPath = "/config/PlexRipperSettings.json";
        var newConfigPath = "/config/TEST_ReaparrSettings.json";
        var oldDbPath = "/config/PlexRipperDB.db";
        var newDbPath = "/config/ReaparrDB.db";

        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigDirectory).Returns(configDir);
        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns("TEST_ReaparrSettings.json");
        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(newConfigPath);
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(newDbPath);

        Mock.Mock<IDirectory>().Setup(x => x.Exists(configDir)).Returns(true);
        Mock.Mock<IPath>().Setup(x => x.Combine(configDir, "PlexRipperSettings.json")).Returns(oldConfigPath);
        Mock.Mock<IPath>().Setup(x => x.Combine(configDir, "PlexRipperDB.db")).Returns(oldDbPath);

        Mock.Mock<IFile>().Setup(x => x.Exists(oldConfigPath)).Returns(true);
        Mock.Mock<IFile>().Setup(x => x.Exists(newConfigPath)).Returns(true); // new already exists

        Mock.Mock<IFile>().Setup(x => x.Exists(oldDbPath)).Returns(true);
        Mock.Mock<IFile>().Setup(x => x.Exists(newDbPath)).Returns(true); // new already exists

        // Ensure setup completes: Config file exists at end
        Mock.Mock<IFile>().Setup(x => x.Exists(newConfigPath)).Returns(true);
        Mock.Mock<IFile>().Setup(x => x.ReadAllText(newConfigPath)).Returns("{}");
        Mock.Mock<IUserSettings>().Setup(x => x.UpdateSettings(It.IsAny<ISettingsModel>())).Returns(new UserSettings());
        Mock.Mock<IFile>().Setup(x => x.Exists(oldDbPath + "-wal")).Returns(false);
        Mock.Mock<IFile>().Setup(x => x.Exists(newDbPath + "-wal")).Returns(false);
        Mock.Mock<IFile>().Setup(x => x.Exists(oldDbPath + "-shm")).Returns(false);
        Mock.Mock<IFile>().Setup(x => x.Exists(newDbPath + "-shm")).Returns(false);

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IFile>().Verify(x => x.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ShouldRenameOnlyWal_WhenOnlyWalPresent()
    {
        // Arrange
        Mock.Mock<IUserSettings>().SetupGet(x => x.SettingsUpdated).Returns(new Subject<UserSettings>());
        var configDir = "/config";
        var oldConfigPath = "/config/PlexRipperSettings.json";
        var newConfigPathForRead = "/config/TEST_ReaparrSettings.json";
        var oldDbPath = "/config/PlexRipperDB.db";
        var newDbPath = "/config/ReaparrDB.db";
        var oldWalPath = oldDbPath + "-wal";
        var newWalPath = newDbPath + "-wal";
        var oldShmPath = oldDbPath + "-shm";
        var newShmPath = newDbPath + "-shm";

        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigDirectory).Returns(configDir);
        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns("TEST_ReaparrSettings.json");
        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(newConfigPathForRead);
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(newDbPath);

        Mock.Mock<IDirectory>().Setup(x => x.Exists(configDir)).Returns(true);
        Mock.Mock<IPath>().Setup(x => x.Combine(configDir, "PlexRipperDB.db")).Returns(oldDbPath);
        Mock.Mock<IPath>().Setup(x => x.Combine(configDir, "PlexRipperSettings.json")).Returns(oldConfigPath);

        Mock.Mock<IFile>().Setup(x => x.Exists(oldDbPath)).Returns(false);
        Mock.Mock<IFile>().Setup(x => x.Exists(newDbPath)).Returns(true);
        Mock.Mock<IFile>().Setup(x => x.Exists(oldConfigPath)).Returns(false);
        Mock.Mock<IFile>().Setup(x => x.Exists(oldWalPath)).Returns(true);
        Mock.Mock<IFile>().Setup(x => x.Exists(newWalPath)).Returns(false);
        Mock.Mock<IFile>().Setup(x => x.Exists(oldShmPath)).Returns(false);
        Mock.Mock<IFile>().Setup(x => x.Exists(newShmPath)).Returns(false);
        Mock.Mock<IFile>().Setup(x => x.Exists(newConfigPathForRead)).Returns(true);
        Mock.Mock<IFile>().Setup(x => x.ReadAllText(newConfigPathForRead)).Returns("{}");
        Mock.Mock<IUserSettings>().Setup(x => x.UpdateSettings(It.IsAny<ISettingsModel>())).Returns(new UserSettings());

        Mock.Mock<IFile>().Setup(x => x.Move(oldWalPath, newWalPath)).Verifiable(Times.Once);
        Mock.Mock<IFile>().Setup(x => x.Move(oldShmPath, newShmPath)).Verifiable(Times.Never);

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IFile>().Verify(x => x.Move(oldWalPath, newWalPath), Times.Once);
        Mock.Mock<IFile>().Verify(x => x.Move(oldShmPath, newShmPath), Times.Never);
    }

    [Fact]
    public void ShouldReturnFailure_WhenConfigDirectoryMissing()
    {
        // Arrange
        Mock.Mock<IUserSettings>().SetupGet(x => x.SettingsUpdated).Returns(new Subject<UserSettings>());
        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns("TEST_ReaparrSettings.json");
        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigDirectory).Returns("/missing");
        Mock.Mock<IDirectory>().Setup(x => x.Exists("/missing")).Returns(false);

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }
}
