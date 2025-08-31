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
        mock.Mock<IUserSettings>().SetupGet(x => x.SettingsUpdated).Returns(new Subject<UserSettings>());
        var configDir = "/config";
        var oldConfigPath = "/config/PlexRipperSettings.json";
        var newConfigPath = "/config/TEST_ReaparrSettings.json";
        var oldDbPath = "/config/PlexRipperDB.db";
        var newDbPath = "/config/ReaparrDB.db";

        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigDirectory).Returns(configDir);
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns("TEST_ReaparrSettings.json");
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(newConfigPath);

        mock.Mock<IDirectory>().Setup(x => x.Exists(configDir)).Returns(true);
        mock.Mock<IPath>().Setup(x => x.Combine(configDir, "PlexRipperSettings.json")).Returns(oldConfigPath);
        mock.Mock<IPath>().Setup(x => x.Combine(configDir, "PlexRipperDB.db")).Returns(oldDbPath);

        mock.Mock<IFile>().Setup(x => x.Exists(oldConfigPath)).Returns(true).Verifiable(Times.Once);
        mock.Mock<IFile>()
            .SetupSequence(x => x.Exists(newConfigPath))
            .Returns(false) // During migration check
            .Returns(true); // During ConfigFileExists at end of Setup

        mock.Mock<IFile>().Setup(x => x.Move(oldConfigPath, newConfigPath)).Verifiable(Times.Once);

        // Additional setups to satisfy strict mocks during migration and load
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(newDbPath);
        mock.Mock<IFile>().Setup(x => x.Exists(oldDbPath)).Returns(false);
        mock.Mock<IFile>().Setup(x => x.Exists(oldDbPath + "-wal")).Returns(false);
        mock.Mock<IFile>().Setup(x => x.Exists(oldDbPath + "-shm")).Returns(false);
        mock.Mock<IFile>().Setup(x => x.ReadAllText(newConfigPath)).Returns("{}");
        mock.Mock<IUserSettings>().Setup(x => x.UpdateSettings(It.IsAny<ISettingsModel>())).Returns(new UserSettings());

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.Mock<IFile>().Verify(x => x.Move(oldConfigPath, newConfigPath), Times.Once);
    }

    [Fact]
    public void ShouldNotThrow_WhenNoLegacyFilesPresent()
    {
        // Arrange
        mock.Mock<IUserSettings>().SetupGet(x => x.SettingsUpdated).Returns(new Subject<UserSettings>());
        var configDir = "/config";
        var newConfigPath = "/config/TEST_ReaparrSettings.json";
        var oldConfigPath = "/config/PlexRipperSettings.json";
        var oldDbPath = "/config/PlexRipperDB.db";
        var newDbPath = "/config/ReaparrDB.db";

        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigDirectory).Returns(configDir);
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns("TEST_ReaparrSettings.json");
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(newConfigPath);
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(newDbPath);
        mock.Mock<IDirectory>().Setup(x => x.Exists(configDir)).Returns(true);
        mock.Mock<IPath>().Setup(x => x.Combine(configDir, "PlexRipperSettings.json")).Returns(oldConfigPath);
        mock.Mock<IPath>().Setup(x => x.Combine(configDir, "PlexRipperDB.db")).Returns(oldDbPath);
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
        mock.Mock<IFile>().Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>()));
        mock.Mock<IFile>().Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>()));
        mock.Mock<IFile>().Setup(x => x.ReadAllText(newConfigPath)).Returns("{}");
        mock.Mock<IUserSettings>().Setup(x => x.UpdateSettings(It.IsAny<ISettingsModel>())).Returns(new UserSettings());

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ShouldRenameLegacyDatabaseFiles_WhenOldDbAndSidecarsExistAndNewMissing()
    {
        // Arrange
        mock.Mock<IUserSettings>().SetupGet(x => x.SettingsUpdated).Returns(new Subject<UserSettings>());
        var configDir = "/config";
        var oldConfigPath = "/config/PlexRipperSettings.json";
        var newConfigPathForRead = "/config/TEST_ReaparrSettings.json";
        var oldDbPath = "/config/PlexRipperDB.db";
        var newDbPath = "/config/ReaparrDB.db";
        var oldWalPath = oldDbPath + "-wal";
        var newWalPath = newDbPath + "-wal";
        var oldShmPath = oldDbPath + "-shm";
        var newShmPath = newDbPath + "-shm";

        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigDirectory).Returns(configDir);
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns("TEST_ReaparrSettings.json");
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(newConfigPathForRead);
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(newDbPath);

        mock.Mock<IDirectory>().Setup(x => x.Exists(configDir)).Returns(true);
        mock.Mock<IPath>().Setup(x => x.Combine(configDir, "PlexRipperSettings.json")).Returns(oldConfigPath);
        mock.Mock<IPath>().Setup(x => x.Combine(configDir, "PlexRipperDB.db")).Returns(oldDbPath);

        // Old exists, new missing initially
        mock.Mock<IFile>().Setup(x => x.Exists(oldDbPath)).Returns(true);
        mock.Mock<IFile>().Setup(x => x.Exists(newDbPath)).Returns(false);

        // Sidecars
        mock.Mock<IFile>().Setup(x => x.Exists(oldWalPath)).Returns(true);
        mock.Mock<IFile>().Setup(x => x.Exists(oldShmPath)).Returns(true);
        mock.Mock<IFile>().Setup(x => x.Exists(newWalPath)).Returns(false);
        mock.Mock<IFile>().Setup(x => x.Exists(newShmPath)).Returns(false);

        // Config file exists check at the end of Setup should pass
        mock.Mock<IFile>().Setup(x => x.Exists(oldConfigPath)).Returns(false);
        mock.Mock<IFile>().Setup(x => x.Exists(newConfigPathForRead)).Returns(true);
        mock.Mock<IFile>().Setup(x => x.ReadAllText(newConfigPathForRead)).Returns("{}");
        mock.Mock<IUserSettings>().Setup(x => x.UpdateSettings(It.IsAny<ISettingsModel>())).Returns(new UserSettings());

        mock.Mock<IFile>().Setup(x => x.Move(oldDbPath, newDbPath)).Verifiable(Times.Once);
        mock.Mock<IFile>().Setup(x => x.Move(oldWalPath, newWalPath)).Verifiable(Times.Once);
        mock.Mock<IFile>().Setup(x => x.Move(oldShmPath, newShmPath)).Verifiable(Times.Once);

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.Mock<IFile>().Verify(x => x.Move(oldDbPath, newDbPath), Times.Once);
        mock.Mock<IFile>().Verify(x => x.Move(oldWalPath, newWalPath), Times.Once);
        mock.Mock<IFile>().Verify(x => x.Move(oldShmPath, newShmPath), Times.Once);
    }

    [Fact]
    public void ShouldNotRename_WhenNewTargetsAlreadyExist()
    {
        // Arrange
        mock.Mock<IUserSettings>().SetupGet(x => x.SettingsUpdated).Returns(new Subject<UserSettings>());
        var configDir = "/config";
        var oldConfigPath = "/config/PlexRipperSettings.json";
        var newConfigPath = "/config/TEST_ReaparrSettings.json";
        var oldDbPath = "/config/PlexRipperDB.db";
        var newDbPath = "/config/ReaparrDB.db";

        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigDirectory).Returns(configDir);
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns("TEST_ReaparrSettings.json");
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(newConfigPath);
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(newDbPath);

        mock.Mock<IDirectory>().Setup(x => x.Exists(configDir)).Returns(true);
        mock.Mock<IPath>().Setup(x => x.Combine(configDir, "PlexRipperSettings.json")).Returns(oldConfigPath);
        mock.Mock<IPath>().Setup(x => x.Combine(configDir, "PlexRipperDB.db")).Returns(oldDbPath);

        mock.Mock<IFile>().Setup(x => x.Exists(oldConfigPath)).Returns(true);
        mock.Mock<IFile>().Setup(x => x.Exists(newConfigPath)).Returns(true); // new already exists

        mock.Mock<IFile>().Setup(x => x.Exists(oldDbPath)).Returns(true);
        mock.Mock<IFile>().Setup(x => x.Exists(newDbPath)).Returns(true); // new already exists

        // Ensure setup completes: Config file exists at end
        mock.Mock<IFile>().Setup(x => x.Exists(newConfigPath)).Returns(true);
        mock.Mock<IFile>().Setup(x => x.ReadAllText(newConfigPath)).Returns("{}");
        mock.Mock<IUserSettings>().Setup(x => x.UpdateSettings(It.IsAny<ISettingsModel>())).Returns(new UserSettings());
        mock.Mock<IFile>().Setup(x => x.Exists(oldDbPath + "-wal")).Returns(false);
        mock.Mock<IFile>().Setup(x => x.Exists(newDbPath + "-wal")).Returns(false);
        mock.Mock<IFile>().Setup(x => x.Exists(oldDbPath + "-shm")).Returns(false);
        mock.Mock<IFile>().Setup(x => x.Exists(newDbPath + "-shm")).Returns(false);

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.Mock<IFile>().Verify(x => x.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ShouldRenameOnlyWal_WhenOnlyWalPresent()
    {
        // Arrange
        mock.Mock<IUserSettings>().SetupGet(x => x.SettingsUpdated).Returns(new Subject<UserSettings>());
        var configDir = "/config";
        var oldConfigPath = "/config/PlexRipperSettings.json";
        var newConfigPathForRead = "/config/TEST_ReaparrSettings.json";
        var oldDbPath = "/config/PlexRipperDB.db";
        var newDbPath = "/config/ReaparrDB.db";
        var oldWalPath = oldDbPath + "-wal";
        var newWalPath = newDbPath + "-wal";
        var oldShmPath = oldDbPath + "-shm";
        var newShmPath = newDbPath + "-shm";

        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigDirectory).Returns(configDir);
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns("TEST_ReaparrSettings.json");
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(newConfigPathForRead);
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(newDbPath);

        mock.Mock<IDirectory>().Setup(x => x.Exists(configDir)).Returns(true);
        mock.Mock<IPath>().Setup(x => x.Combine(configDir, "PlexRipperDB.db")).Returns(oldDbPath);
        mock.Mock<IPath>().Setup(x => x.Combine(configDir, "PlexRipperSettings.json")).Returns(oldConfigPath);

        mock.Mock<IFile>().Setup(x => x.Exists(oldDbPath)).Returns(false);
        mock.Mock<IFile>().Setup(x => x.Exists(newDbPath)).Returns(true);
        mock.Mock<IFile>().Setup(x => x.Exists(oldConfigPath)).Returns(false);
        mock.Mock<IFile>().Setup(x => x.Exists(oldWalPath)).Returns(true);
        mock.Mock<IFile>().Setup(x => x.Exists(newWalPath)).Returns(false);
        mock.Mock<IFile>().Setup(x => x.Exists(oldShmPath)).Returns(false);
        mock.Mock<IFile>().Setup(x => x.Exists(newShmPath)).Returns(false);
        mock.Mock<IFile>().Setup(x => x.Exists(newConfigPathForRead)).Returns(true);
        mock.Mock<IFile>().Setup(x => x.ReadAllText(newConfigPathForRead)).Returns("{}");
        mock.Mock<IUserSettings>().Setup(x => x.UpdateSettings(It.IsAny<ISettingsModel>())).Returns(new UserSettings());

        mock.Mock<IFile>().Setup(x => x.Move(oldWalPath, newWalPath)).Verifiable(Times.Once);
        mock.Mock<IFile>().Setup(x => x.Move(oldShmPath, newShmPath)).Verifiable(Times.Never);

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.Mock<IFile>().Verify(x => x.Move(oldWalPath, newWalPath), Times.Once);
        mock.Mock<IFile>().Verify(x => x.Move(oldShmPath, newShmPath), Times.Never);
    }

    [Fact]
    public void ShouldReturnFailure_WhenConfigDirectoryMissing()
    {
        // Arrange
        mock.Mock<IUserSettings>().SetupGet(x => x.SettingsUpdated).Returns(new Subject<UserSettings>());
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns("TEST_ReaparrSettings.json");
        mock.Mock<IPathProvider>().SetupGet(x => x.ConfigDirectory).Returns("/missing");
        mock.Mock<IDirectory>().Setup(x => x.Exists("/missing")).Returns(false);

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }
}
