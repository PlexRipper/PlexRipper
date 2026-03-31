using System.IO.Abstractions.TestingHelpers;
using System.Reactive.Subjects;
using Reaparr.Environment;
using Reaparr.Settings.Contracts;

namespace Reaparr.Settings.UnitTests;

public class ConfigManagerMigrateLegacyFileNamesUnitTests : BaseUnitTest<ConfigManager>
{
    private const string ConfigDirectory = "/config";
    private const string LegacyConfigPath = "/config/PlexRipperSettings.json";
    private const string ConfigPath = "/config/TEST_ReaparrSettings.json";
    private const string LegacyDatabasePath = "/config/PlexRipperDB.db";
    private const string DatabasePath = "/config/ReaparrDB.db";

    private void SetupDependencies()
    {
        Mock.Mock<IUserSettings>().SetupGet(x => x.SettingsUpdated).Returns(new Subject<UserSettings>());
        Mock.Mock<IUserSettings>().Setup(x => x.UpdateSettings(It.IsAny<ISettingsModel>())).Returns(new UserSettings());
        Mock.Mock<IUserSettings>().Setup(x => x.Reset());

        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigDirectory).Returns(ConfigDirectory);
        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns("TEST_ReaparrSettings.json");
        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(ConfigPath);
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(DatabasePath);
    }

    [Test]
    public void ShouldRenameLegacyConfigFile_WhenOldExistsAndNewMissing()
    {
        // Arrange
        MockFileSystem? fileSystem = null;
        SetupFileSystem(system =>
        {
            fileSystem = system;
            system.AddDirectory(ConfigDirectory);
            system.AddFile(LegacyConfigPath, new MockFileData("{}"));
        });
        SetupDependencies();

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        fileSystem.ShouldNotBeNull();
        fileSystem.FileExists(LegacyConfigPath).ShouldBeFalse();
        fileSystem.FileExists(ConfigPath).ShouldBeTrue();
    }

    [Test]
    public void ShouldNotThrow_WhenNoLegacyFilesPresent()
    {
        // Arrange
        MockFileSystem? fileSystem = null;
        SetupFileSystem(system =>
        {
            fileSystem = system;
            system.AddDirectory(ConfigDirectory);
        });
        SetupDependencies();

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        fileSystem.ShouldNotBeNull();
        fileSystem.FileExists(ConfigPath).ShouldBeTrue();
    }

    [Test]
    public void ShouldRenameLegacyDatabaseFiles_WhenOldDbAndSidecarsExistAndNewMissing()
    {
        // Arrange
        var legacyWalPath = LegacyDatabasePath + "-wal";
        var legacyShmPath = LegacyDatabasePath + "-shm";
        var walPath = DatabasePath + "-wal";
        var shmPath = DatabasePath + "-shm";

        MockFileSystem? fileSystem = null;
        SetupFileSystem(system =>
        {
            fileSystem = system;
            system.AddDirectory(ConfigDirectory);
            system.AddFile(ConfigPath, new MockFileData("{}"));
            system.AddFile(LegacyDatabasePath, new MockFileData(string.Empty));
            system.AddFile(legacyWalPath, new MockFileData(string.Empty));
            system.AddFile(legacyShmPath, new MockFileData(string.Empty));
        });
        SetupDependencies();

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        fileSystem.ShouldNotBeNull();
        fileSystem.FileExists(LegacyDatabasePath).ShouldBeFalse();
        fileSystem.FileExists(legacyWalPath).ShouldBeFalse();
        fileSystem.FileExists(legacyShmPath).ShouldBeFalse();
        fileSystem.FileExists(DatabasePath).ShouldBeTrue();
        fileSystem.FileExists(walPath).ShouldBeTrue();
        fileSystem.FileExists(shmPath).ShouldBeTrue();
    }

    [Test]
    public void ShouldNotRename_WhenNewTargetsAlreadyExist()
    {
        // Arrange
        MockFileSystem? fileSystem = null;
        SetupFileSystem(system =>
        {
            fileSystem = system;
            system.AddDirectory(ConfigDirectory);
            system.AddFile(LegacyConfigPath, new MockFileData("legacy"));
            system.AddFile(ConfigPath, new MockFileData("{}"));
            system.AddFile(LegacyDatabasePath, new MockFileData("legacy-db"));
            system.AddFile(DatabasePath, new MockFileData("new-db"));
        });
        SetupDependencies();

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        fileSystem.ShouldNotBeNull();
        fileSystem.GetFile(LegacyConfigPath).TextContents.ShouldBe("legacy");
        fileSystem.GetFile(ConfigPath).TextContents.ShouldBe("{}");
        fileSystem.GetFile(LegacyDatabasePath).TextContents.ShouldBe("legacy-db");
        fileSystem.GetFile(DatabasePath).TextContents.ShouldBe("new-db");
    }

    [Test]
    public void ShouldRenameOnlyWal_WhenOnlyWalPresent()
    {
        // Arrange
        var legacyWalPath = LegacyDatabasePath + "-wal";
        var walPath = DatabasePath + "-wal";
        var shmPath = DatabasePath + "-shm";

        MockFileSystem? fileSystem = null;
        SetupFileSystem(system =>
        {
            fileSystem = system;
            system.AddDirectory(ConfigDirectory);
            system.AddFile(ConfigPath, new MockFileData("{}"));
            system.AddFile(DatabasePath, new MockFileData("db"));
            system.AddFile(legacyWalPath, new MockFileData("wal"));
        });
        SetupDependencies();

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        fileSystem.ShouldNotBeNull();
        fileSystem.FileExists(legacyWalPath).ShouldBeFalse();
        fileSystem.FileExists(walPath).ShouldBeTrue();
        fileSystem.FileExists(shmPath).ShouldBeFalse();
    }

    [Test]
    public void ShouldCreateConfigDirectory_WhenConfigDirectoryMissing()
    {
        // Arrange
        MockFileSystem? fileSystem = null;
        SetupFileSystem(system => fileSystem = system);
        Mock.Mock<IUserSettings>().SetupGet(x => x.SettingsUpdated).Returns(new Subject<UserSettings>());
        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns("TEST_ReaparrSettings.json");
        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns("/missing/TEST_ReaparrSettings.json");
        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigDirectory).Returns("/missing");

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        fileSystem.ShouldNotBeNull();
        fileSystem.Directory.Exists("/missing").ShouldBeTrue();
        fileSystem.FileExists("/missing/TEST_ReaparrSettings.json").ShouldBeTrue();
    }
}
