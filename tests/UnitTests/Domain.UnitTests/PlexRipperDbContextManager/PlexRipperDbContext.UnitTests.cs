using System.IO.Abstractions;
using Data.Contracts;
using Environment;
using PlexRipper.Data;
using PlexRipper.Identity.Contracts;
using Settings.Contracts;

namespace Domain.UnitTests;

public class PlexRipperDbContextManagerUnitTests : BaseUnitTest<PlexRipperDbContextManager>
{
    private string DatabasePath => "/Config/" + PathProvider.DatabaseName;

    public PlexRipperDbContextManagerUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldConnectToDatabaseAndCheckToMigrate_WhenDatabaseAlreadyExists()
    {
        // Arrange

        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CanConnect()).Returns(true);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns([]);
        mock.Mock<IAuthDbContextDatabase>().Setup(x => x.CanConnect()).Returns(true);
        mock.Mock<IAuthDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        mock.Mock<IAuthDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns([]);

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.Mock<IPlexRipperDbContextDatabase>().Verify(x => x.CanConnect(), Times.Once);
        mock.Mock<IPlexRipperDbContextDatabase>().Verify(x => x.GetPendingMigrations(), Times.Once);
        mock.Mock<IPlexRipperDbContextDatabase>().Verify(x => x.Migrate(), Times.Never);
        mock.Mock<IAuthDbContextDatabase>().Verify(x => x.CanConnect(), Times.Never);
        mock.Mock<IAuthDbContextDatabase>().Verify(x => x.GetPendingMigrations(), Times.Once);
        mock.Mock<IAuthDbContextDatabase>().Verify(x => x.Migrate(), Times.Never);
    }

    [Fact]
    public void ShouldCreateDatabase_WhenDatabaseDoesNotExist()
    {
        // Arrange
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(false).Verifiable(Times.Once);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok()).Verifiable(Times.Once);
        mock.Mock<IAuthDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok()).Verifiable(Times.Once);

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.Mock<IPlexRipperDbContextDatabase>().Verify(x => x.Migrate(), Times.Once); // Database creation involves migration
    }

    [Fact]
    public void ShouldLogWarning_WhenDatabaseDoesNotExist()
    {
        // Arrange
        mock.Mock<IPathProvider>()
            .SetupGet(x => x.DatabasePath)
            .Returns(() => DatabasePath)
            .Verifiable(Times.Exactly(2));
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(false).Verifiable(Times.Once);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok()).Verifiable(Times.Once);
        mock.Mock<IAuthDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok()).Verifiable(Times.Once);

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ShouldFailToCreateDatabase_WhenExceptionIsThrown()
    {
        // Arrange
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
        mock.Mock<IPlexRipperDbContextDatabase>()
            .Setup(x => x.Migrate())
            .Throws(new Exception("Test Exception"))
            .Verifiable(Times.Once);
        mock.Mock<IAuthDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok()).Verifiable(Times.Never);

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public void ShouldBackUpAndResetDatabase_WhenDatabaseCannotConnect()
    {
        // Arrange
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabaseBackupDirectory).Returns(() => DatabasePath);
        mock.Mock<IPathProvider>()
            .Setup(x => x.DatabaseFiles)
            .Returns(() => [PathProvider.DatabasePath, PathProvider.Database_SHM_Path, PathProvider.Database_WAL_Path]);
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        mock.Mock<IFile>().Setup(x => x.Copy(It.IsAny<string>(), It.IsAny<string>())).Verifiable(Times.Exactly(3));
        mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok()).Verifiable(Times.Once);
        mock.Mock<IAuthDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok()).Verifiable(Times.Once);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CanConnect()).Returns(false);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.EnsureDeleted()).Returns(Result.Ok(true));
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CloseConnection());
        mock.Mock<IGeneralSettings>().SetupSet(x => x.FirstTimeSetup = true).Verifiable(Times.Once);

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var mockDbContext = mock.Mock<IPlexRipperDbContextDatabase>();
        mockDbContext.Verify(x => x.CanConnect(), Times.Once);
        mockDbContext.Verify(x => x.EnsureDeleted(), Times.Once); // Database is reset
        mockDbContext.Verify(x => x.Migrate(), Times.Once); // Database is recreated after reset
    }

    [Fact]
    public void ShouldMigratePlexRipperDatabase_WhenPendingMigrationsExist()
    {
        // Arrange
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CanConnect()).Returns(true);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        mock.Mock<IPlexRipperDbContextDatabase>()
            .Setup(x => x.GetPendingMigrations())
            .Returns(["Migration1", "Migration2"]);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok()).Verifiable(Times.Once);
        mock.Mock<IAuthDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        mock.Mock<IAuthDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns([]);

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.Mock<IPlexRipperDbContextDatabase>().Verify(x => x.Migrate(), Times.Once);
        mock.Mock<IAuthDbContextDatabase>().Verify(x => x.Migrate(), Times.Never);
    }

    [Fact]
    public void ShouldMigrateAuthDatabase_WhenPendingMigrationsExist()
    {
        // Arrange
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CanConnect()).Returns(true);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns([]);
        mock.Mock<IAuthDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        mock.Mock<IAuthDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns(["AuthMigration1"]);
        mock.Mock<IAuthDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok()).Verifiable(Times.Once);

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.Mock<IPlexRipperDbContextDatabase>().Verify(x => x.Migrate(), Times.Never);
        mock.Mock<IAuthDbContextDatabase>().Verify(x => x.Migrate(), Times.Once);
    }

    [Fact]
    public void ShouldSkipMigration_WhenDatabaseIsInMemory()
    {
        // Arrange
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CanConnect()).Returns(true);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(true);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns(["Migration1"]);
        mock.Mock<IAuthDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(true);
        mock.Mock<IAuthDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns(["AuthMigration1"]);

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.Mock<IPlexRipperDbContextDatabase>().Verify(x => x.Migrate(), Times.Never);
        mock.Mock<IAuthDbContextDatabase>().Verify(x => x.Migrate(), Times.Never);
    }

    [Fact]
    public void ShouldResetDatabase_WhenPlexRipperMigrationFails()
    {
        // Arrange
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabaseBackupDirectory).Returns(() => "/backup");
        mock.Mock<IPathProvider>().Setup(x => x.DatabaseFiles).Returns(() => [DatabasePath]);
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        mock.Mock<IFile>().Setup(x => x.Copy(It.IsAny<string>(), It.IsAny<string>()));
        mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object);

        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CanConnect()).Returns(true);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns(["Migration1"]);
        mock.Mock<IPlexRipperDbContextDatabase>()
            .SetupSequence(x => x.Migrate())
            .Returns(Result.Fail("Migration failed"))
            .Returns(Result.Ok()); // Second call during database reset
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CloseConnection());
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.EnsureDeleted()).Returns(Result.Ok(true));
        mock.Mock<IAuthDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok());
        mock.Mock<IAuthDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        mock.Mock<IAuthDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns([]);
        mock.Mock<IGeneralSettings>().SetupSet(x => x.FirstTimeSetup = true).Verifiable(Times.AtLeastOnce);

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.Mock<IPlexRipperDbContextDatabase>().Verify(x => x.Migrate(), Times.Exactly(2)); // Once for a migration attempt and once for a reset
        mock.Mock<IPlexRipperDbContextDatabase>().Verify(x => x.EnsureDeleted(), Times.Once);
    }

    [Fact]
    public void ShouldResetDatabase_WhenAuthDatabaseMigrationFails()
    {
        // Arrange
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabaseBackupDirectory).Returns(() => "/backup");
        mock.Mock<IPathProvider>().Setup(x => x.DatabaseFiles).Returns(() => [DatabasePath]);
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        mock.Mock<IFile>().Setup(x => x.Copy(It.IsAny<string>(), It.IsAny<string>()));
        mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object);

        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CanConnect()).Returns(true);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns([]);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok());
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CloseConnection());
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.EnsureDeleted()).Returns(Result.Ok(true));

        mock.Mock<IAuthDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        mock.Mock<IAuthDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns(["AuthMigration1"]);
        mock.Mock<IAuthDbContextDatabase>()
            .SetupSequence(x => x.Migrate())
            .Returns(Result.Fail("Auth migration failed"))
            .Returns(Result.Ok()); // Second call during database reset
        mock.Mock<IGeneralSettings>().SetupSet(x => x.FirstTimeSetup = true).Verifiable(Times.AtLeastOnce);

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.Mock<IAuthDbContextDatabase>().Verify(x => x.Migrate(), Times.Exactly(2)); // Once for migration attempt, once for reset
        mock.Mock<IPlexRipperDbContextDatabase>().Verify(x => x.EnsureDeleted(), Times.Once);
    }

    [Fact]
    public void ShouldResetDatabase_WhenMigrationThrowsException()
    {
        // Arrange
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabaseBackupDirectory).Returns(() => "/backup");
        mock.Mock<IPathProvider>().Setup(x => x.DatabaseFiles).Returns(() => [DatabasePath]);
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        mock.Mock<IFile>().Setup(x => x.Copy(It.IsAny<string>(), It.IsAny<string>()));
        mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object);

        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CanConnect()).Returns(true);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns(["Migration1"]);
        mock.Mock<IPlexRipperDbContextDatabase>()
            .SetupSequence(x => x.Migrate())
            .Throws(new Exception("Migration exception"))
            .Returns(Result.Ok()); // Second call during database reset
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CloseConnection());
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.EnsureDeleted()).Returns(Result.Ok(true));
        mock.Mock<IAuthDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok());
        mock.Mock<IAuthDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        mock.Mock<IAuthDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns([]);
        mock.Mock<IGeneralSettings>().SetupSet(x => x.FirstTimeSetup = true).Verifiable(Times.Once);

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.Mock<IPlexRipperDbContextDatabase>().Verify(x => x.EnsureDeleted(), Times.Once);
    }

    [Fact]
    public void ShouldFailBackup_WhenBackupDirectoryCannotBeCreated()
    {
        // Arrange
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabaseBackupDirectory).Returns(() => "/backup");
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Throws(new Exception("Directory creation failed"));
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CanConnect()).Returns(false);

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public void ShouldHandlePartialBackup_WhenSomeDatabaseFilesAreMissing()
    {
        // Arrange
        List<string> dbFiles = [DatabasePath, DatabasePath + "-shm", DatabasePath + "-wal"];
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabaseBackupDirectory).Returns(() => "/backup");
        mock.Mock<IPathProvider>().Setup(x => x.DatabaseFiles).Returns(() => dbFiles);
        mock.Mock<IFile>().Setup(x => x.Exists(DatabasePath)).Returns(true);
        mock.Mock<IFile>().Setup(x => x.Exists(DatabasePath + "-shm")).Returns(false);
        mock.Mock<IFile>().Setup(x => x.Exists(DatabasePath + "-wal")).Returns(true);
        mock.Mock<IFile>().Setup(x => x.Copy(It.IsAny<string>(), It.IsAny<string>()));
        mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object);

        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CanConnect()).Returns(false);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CloseConnection());
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.EnsureDeleted()).Returns(Result.Ok(true));
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok());
        mock.Mock<IAuthDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok());
        mock.Mock<IGeneralSettings>().SetupSet(x => x.FirstTimeSetup = true).Verifiable(Times.Once);

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.Mock<IFile>().Verify(x => x.Copy(DatabasePath, It.IsAny<string>()), Times.Once);
        mock.Mock<IFile>().Verify(x => x.Copy(DatabasePath + "-wal", It.IsAny<string>()), Times.Once);
        mock.Mock<IFile>().Verify(x => x.Copy(DatabasePath + "-shm", It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ShouldFailReset_WhenDatabaseDeletionFails()
    {
        // Arrange
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabaseBackupDirectory).Returns(() => "/backup");
        mock.Mock<IPathProvider>().Setup(x => x.DatabaseFiles).Returns(() => [DatabasePath]);
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        mock.Mock<IFile>().Setup(x => x.Copy(It.IsAny<string>(), It.IsAny<string>()));
        mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object);

        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CanConnect()).Returns(false);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CloseConnection());
        mock.Mock<IPlexRipperDbContextDatabase>()
            .Setup(x => x.EnsureDeleted())
            .Returns(Result.Fail("Database deletion failed"));

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsFailed.ShouldBeTrue();
        mock.Mock<IPlexRipperDbContextDatabase>().Verify(x => x.EnsureDeleted(), Times.Once);
        mock.Mock<IPlexRipperDbContextDatabase>().Verify(x => x.Migrate(), Times.Never); // Should not attempt to recreate if deletion failed
    }

    [Fact]
    public void ShouldFailReset_WhenDatabaseCreationFailsAfterReset()
    {
        // Arrange
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabaseBackupDirectory).Returns(() => "/backup");
        mock.Mock<IPathProvider>().Setup(x => x.DatabaseFiles).Returns(() => [DatabasePath]);
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        mock.Mock<IFile>().Setup(x => x.Copy(It.IsAny<string>(), It.IsAny<string>()));
        mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object);

        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CanConnect()).Returns(false);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CloseConnection());
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.EnsureDeleted()).Returns(Result.Ok(true));
        mock.Mock<IPlexRipperDbContextDatabase>()
            .Setup(x => x.Migrate())
            .Throws(new Exception("Database creation failed"));

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsFailed.ShouldBeTrue();
        mock.Mock<IPlexRipperDbContextDatabase>().Verify(x => x.EnsureDeleted(), Times.Once);
        mock.Mock<IPlexRipperDbContextDatabase>().Verify(x => x.Migrate(), Times.Once);
    }

    [Fact]
    public void ShouldHandleExceptionInResetDatabase_AndReturnFailure()
    {
        // Arrange
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CanConnect()).Returns(false);
        mock.Mock<IPlexRipperDbContextDatabase>()
            .Setup(x => x.CloseConnection())
            .Throws(new Exception("Connection close failed"));
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public void ShouldSetFirstTimeSetupToTrue_WhenDatabaseIsReset()
    {
        // Arrange
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabaseBackupDirectory).Returns(() => "/backup");
        mock.Mock<IPathProvider>().Setup(x => x.DatabaseFiles).Returns(() => [DatabasePath]);
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        mock.Mock<IFile>().Setup(x => x.Copy(It.IsAny<string>(), It.IsAny<string>()));
        mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object);

        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CanConnect()).Returns(false);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CloseConnection());
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.EnsureDeleted()).Returns(Result.Ok(true));
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok());
        mock.Mock<IAuthDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok());

        var generalSettingsMock = mock.Mock<IGeneralSettings>();
        generalSettingsMock.SetupProperty(x => x.FirstTimeSetup);

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        generalSettingsMock.VerifySet(x => x.FirstTimeSetup = true, Times.Once);
    }

    [Fact]
    public void ShouldSkipBackup_WhenDatabaseDoesNotExistDuringReset()
    {
        // Arrange
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        mock.Mock<IFile>().Setup(x => x.Exists(DatabasePath)).Returns(false); // Database doesn't exist for backup
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CanConnect()).Returns(false);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CloseConnection());
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.EnsureDeleted()).Returns(Result.Ok(true));
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok());
        mock.Mock<IAuthDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok());

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.Mock<IDirectory>().Verify(x => x.CreateDirectory(It.IsAny<string>()), Times.Never);
        mock.Mock<IFile>().Verify(x => x.Copy(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ShouldFailAuthDatabaseCreation_WhenExceptionIsThrown()
    {
        // Arrange
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok());
        mock.Mock<IAuthDbContextDatabase>()
            .Setup(x => x.Migrate())
            .Throws(new Exception("Auth database creation failed"));

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsFailed.ShouldBeTrue();
        mock.Mock<IPlexRipperDbContextDatabase>().Verify(x => x.Migrate(), Times.Once);
        mock.Mock<IAuthDbContextDatabase>().Verify(x => x.Migrate(), Times.Once);
    }
}
