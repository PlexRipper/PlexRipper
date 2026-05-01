using System.IO.Abstractions.TestingHelpers;
using Autofac;
using Reaparr.Environment;
using Reaparr.Settings.Contracts;

namespace Reaparr.Settings.UnitTests;

public class ConfigManagerMigrateLegacyFileNamesUnitTests : BaseUnitTest<ConfigManager>
{
    private readonly IPathProvider _pathProvider;

    public ConfigManagerMigrateLegacyFileNamesUnitTests()
    {
        _pathProvider = Mock.Container.Resolve<IPathProvider>();
    }

    private string ConfigDirectory => _pathProvider.ConfigDirectory;
    private string LegacyConfigPath => Path.Combine(ConfigDirectory, "PlexRipperSettings.json");
    private string ConfigPath => _pathProvider.ConfigFileLocation;
    private string LegacyDatabasePath => Path.Combine(ConfigDirectory, "PlexRipperDB.db");
    private string DatabasePath => _pathProvider.DatabasePath;

    [Test]
    public void ShouldRenameLegacyConfigFile_WhenOldExistsAndNewMissing()
    {
        // Arrange
        MockFileSystem? fileSystem = null;
        SetupDependencies(builder => builder.RegisterInstance<IUserSettings>(new UserSettings()));
        SetupFileSystem(system =>
        {
            fileSystem = system;
            system.AddDirectory(ConfigDirectory);
            system.AddFile(LegacyConfigPath, new MockFileData("{}"));
        });

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
        SetupDependencies(builder => builder.RegisterInstance<IUserSettings>(new UserSettings()));
        SetupFileSystem(system =>
        {
            fileSystem = system;
            system.AddDirectory(ConfigDirectory);
        });

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
        SetupDependencies(builder => builder.RegisterInstance<IUserSettings>(new UserSettings()));
        SetupFileSystem(system =>
        {
            fileSystem = system;
            system.AddDirectory(ConfigDirectory);
            system.AddFile(ConfigPath, new MockFileData("{}"));
            system.AddFile(LegacyDatabasePath, new MockFileData(string.Empty));
            system.AddFile(legacyWalPath, new MockFileData(string.Empty));
            system.AddFile(legacyShmPath, new MockFileData(string.Empty));
        });

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
        SetupDependencies(builder => builder.RegisterInstance<IUserSettings>(new UserSettings()));
        SetupFileSystem(system =>
        {
            fileSystem = system;
            system.AddDirectory(ConfigDirectory);
            system.AddFile(LegacyConfigPath, new MockFileData("legacy"));
            system.AddFile(ConfigPath, new MockFileData("{}"));
            system.AddFile(LegacyDatabasePath, new MockFileData("legacy-db"));
            system.AddFile(DatabasePath, new MockFileData("new-db"));
        });

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
        SetupDependencies(builder => builder.RegisterInstance<IUserSettings>(new UserSettings()));
        SetupFileSystem(system =>
        {
            fileSystem = system;
            system.AddDirectory(ConfigDirectory);
            system.AddFile(ConfigPath, new MockFileData("{}"));
            system.AddFile(DatabasePath, new MockFileData("db"));
            system.AddFile(legacyWalPath, new MockFileData("wal"));
        });

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
        SetupDependencies(builder => builder.RegisterInstance<IUserSettings>(new UserSettings()));
        SetupFileSystem(system => fileSystem = system);

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        fileSystem.ShouldNotBeNull();
        fileSystem.Directory.Exists(ConfigDirectory).ShouldBeTrue();
        fileSystem.FileExists(ConfigPath).ShouldBeTrue();
    }
}
