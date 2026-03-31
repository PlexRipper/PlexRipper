using System.IO.Abstractions.TestingHelpers;
using System.Reactive.Subjects;
using Reaparr.Environment;
using Reaparr.Settings.Contracts;

namespace Reaparr.Settings.UnitTests;

public class ConfigManagerMigrateLegacyFileNamesUnitTests : BaseUnitTest<ConfigManager>
{
    private const string CONFIG_DIRECTORY = "/config";
    private const string LEGACY_CONFIG_PATH = "/config/PlexRipperSettings.json";
    private const string CONFIG_PATH = "/config/TEST_ReaparrSettings.json";
    private const string LEGACY_DATABASE_PATH = "/config/PlexRipperDB.db";
    private const string DATABASE_PATH = "/config/ReaparrDB.db";

    private void SetupDependencies()
    {
        Mock.Mock<IUserSettings>().SetupGet(x => x.SettingsUpdated).Returns(new Subject<UserSettings>());
        Mock.Mock<IUserSettings>().Setup(x => x.UpdateSettings(It.IsAny<ISettingsModel>())).Returns(new UserSettings());
        Mock.Mock<IUserSettings>().Setup(x => x.Reset());

        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigDirectory).Returns(CONFIG_DIRECTORY);
        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileName).Returns("TEST_ReaparrSettings.json");
        Mock.Mock<IPathProvider>().SetupGet(x => x.ConfigFileLocation).Returns(CONFIG_PATH);
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(DATABASE_PATH);
    }

    [Test]
    public void ShouldRenameLegacyConfigFile_WhenOldExistsAndNewMissing()
    {
        // Arrange
        MockFileSystem? fileSystem = null;
        SetupFileSystem(system =>
        {
            fileSystem = system;
            system.AddDirectory(CONFIG_DIRECTORY);
            system.AddFile(LEGACY_CONFIG_PATH, new MockFileData("{}"));
        });
        SetupDependencies();

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        fileSystem.ShouldNotBeNull();
        fileSystem.FileExists(LEGACY_CONFIG_PATH).ShouldBeFalse();
        fileSystem.FileExists(CONFIG_PATH).ShouldBeTrue();
    }

    [Test]
    public void ShouldNotThrow_WhenNoLegacyFilesPresent()
    {
        // Arrange
        MockFileSystem? fileSystem = null;
        SetupFileSystem(system =>
        {
            fileSystem = system;
            system.AddDirectory(CONFIG_DIRECTORY);
        });
        SetupDependencies();

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        fileSystem.ShouldNotBeNull();
        fileSystem.FileExists(CONFIG_PATH).ShouldBeTrue();
    }

    [Test]
    public void ShouldRenameLegacyDatabaseFiles_WhenOldDbAndSidecarsExistAndNewMissing()
    {
        // Arrange
        var legacyWalPath = LEGACY_DATABASE_PATH + "-wal";
        var legacyShmPath = LEGACY_DATABASE_PATH + "-shm";
        var walPath = DATABASE_PATH + "-wal";
        var shmPath = DATABASE_PATH + "-shm";

        MockFileSystem? fileSystem = null;
        SetupFileSystem(system =>
        {
            fileSystem = system;
            system.AddDirectory(CONFIG_DIRECTORY);
            system.AddFile(CONFIG_PATH, new MockFileData("{}"));
            system.AddFile(LEGACY_DATABASE_PATH, new MockFileData(string.Empty));
            system.AddFile(legacyWalPath, new MockFileData(string.Empty));
            system.AddFile(legacyShmPath, new MockFileData(string.Empty));
        });
        SetupDependencies();

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        fileSystem.ShouldNotBeNull();
        fileSystem.FileExists(LEGACY_DATABASE_PATH).ShouldBeFalse();
        fileSystem.FileExists(legacyWalPath).ShouldBeFalse();
        fileSystem.FileExists(legacyShmPath).ShouldBeFalse();
        fileSystem.FileExists(DATABASE_PATH).ShouldBeTrue();
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
            system.AddDirectory(CONFIG_DIRECTORY);
            system.AddFile(LEGACY_CONFIG_PATH, new MockFileData("legacy"));
            system.AddFile(CONFIG_PATH, new MockFileData("{}"));
            system.AddFile(LEGACY_DATABASE_PATH, new MockFileData("legacy-db"));
            system.AddFile(DATABASE_PATH, new MockFileData("new-db"));
        });
        SetupDependencies();

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        fileSystem.ShouldNotBeNull();
        fileSystem.GetFile(LEGACY_CONFIG_PATH).TextContents.ShouldBe("legacy");
        fileSystem.GetFile(CONFIG_PATH).TextContents.ShouldBe("{}");
        fileSystem.GetFile(LEGACY_DATABASE_PATH).TextContents.ShouldBe("legacy-db");
        fileSystem.GetFile(DATABASE_PATH).TextContents.ShouldBe("new-db");
    }

    [Test]
    public void ShouldRenameOnlyWal_WhenOnlyWalPresent()
    {
        // Arrange
        var legacyWalPath = LEGACY_DATABASE_PATH + "-wal";
        var walPath = DATABASE_PATH + "-wal";
        var shmPath = DATABASE_PATH + "-shm";

        MockFileSystem? fileSystem = null;
        SetupFileSystem(system =>
        {
            fileSystem = system;
            system.AddDirectory(CONFIG_DIRECTORY);
            system.AddFile(CONFIG_PATH, new MockFileData("{}"));
            system.AddFile(DATABASE_PATH, new MockFileData("db"));
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
