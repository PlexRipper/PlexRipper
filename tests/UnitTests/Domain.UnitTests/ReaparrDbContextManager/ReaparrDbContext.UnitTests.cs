using System.IO.Abstractions;
using Reaparr.Data;
using Reaparr.Data.Contracts;
using Reaparr.Environment;
using Reaparr.Identity.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Domain.UnitTests;

public class ReaparrDbContextManagerUnitTests : BaseUnitTest<ReaparrDbContextManager>
{
    private string DatabasePath => "/Config/" + PathProvider.DatabaseName;

    public ReaparrDbContextManagerUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldConnectToDatabaseAndCheckToMigrate_WhenDatabaseAlreadyExists()
    {
        // Arrange

        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        Mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.CanConnect()).Returns(true);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns([]);
        Mock.Mock<IAuthDbContextDatabase>().Setup(x => x.CanConnect()).Returns(true);
        Mock.Mock<IAuthDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        Mock.Mock<IAuthDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns([]);

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IReaparrDbContextDatabase>().Verify(x => x.CanConnect(), Times.Once);
        Mock.Mock<IReaparrDbContextDatabase>().Verify(x => x.GetPendingMigrations(), Times.Once);
        Mock.Mock<IReaparrDbContextDatabase>().Verify(x => x.Migrate(), Times.Never);
        Mock.Mock<IAuthDbContextDatabase>().Verify(x => x.CanConnect(), Times.Never);
        Mock.Mock<IAuthDbContextDatabase>().Verify(x => x.GetPendingMigrations(), Times.Once);
        Mock.Mock<IAuthDbContextDatabase>().Verify(x => x.Migrate(), Times.Never);
    }

    [Fact]
    public void ShouldCreateDatabase_WhenDatabaseDoesNotExist()
    {
        // Arrange
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        Mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(false).Verifiable(Times.Once);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok()).Verifiable(Times.Once);
        Mock.Mock<IAuthDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok()).Verifiable(Times.Once);

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IReaparrDbContextDatabase>().Verify(x => x.Migrate(), Times.Once); // Database creation involves migration
    }

    [Fact]
    public void ShouldLogWarning_WhenDatabaseDoesNotExist()
    {
        // Arrange
        Mock.Mock<IPathProvider>()
            .SetupGet(x => x.DatabasePath)
            .Returns(() => DatabasePath)
            .Verifiable(Times.Exactly(2));
        Mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(false).Verifiable(Times.Once);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok()).Verifiable(Times.Once);
        Mock.Mock<IAuthDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok()).Verifiable(Times.Once);

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ShouldFailToCreateDatabase_WhenExceptionIsThrown()
    {
        // Arrange
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        Mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
        Mock.Mock<IReaparrDbContextDatabase>()
            .Setup(x => x.Migrate())
            .Throws(new Exception("Test Exception"))
            .Verifiable(Times.Once);
        Mock.Mock<IAuthDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok()).Verifiable(Times.Never);

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public void ShouldBackUpAndResetDatabase_WhenDatabaseCannotConnect()
    {
        // Arrange
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabaseBackupDirectory).Returns(() => DatabasePath);
        Mock.Mock<IPathProvider>()
            .Setup(x => x.DatabaseFiles)
            .Returns(() => [PathProvider.DatabasePath, PathProvider.Database_SHM_Path, PathProvider.Database_WAL_Path]);
        Mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        Mock.Mock<IFile>().Setup(x => x.Copy(It.IsAny<string>(), It.IsAny<string>())).Verifiable(Times.Exactly(3));
        Mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok()).Verifiable(Times.Once);
        Mock.Mock<IAuthDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok()).Verifiable(Times.Once);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.CanConnect()).Returns(false);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.EnsureDeleted()).Returns(Result.Ok(true));
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.CloseConnection());
        Mock.Mock<IGeneralSettings>().SetupSet(x => x.FirstTimeSetup = true).Verifiable(Times.Once);

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var mockDbContext = Mock.Mock<IReaparrDbContextDatabase>();
        mockDbContext.Verify(x => x.CanConnect(), Times.Once);
        mockDbContext.Verify(x => x.EnsureDeleted(), Times.Once); // Database is reset
        mockDbContext.Verify(x => x.Migrate(), Times.Once); // Database is recreated after reset
    }

    [Fact]
    public void ShouldMigrateReaparrDatabase_WhenPendingMigrationsExist()
    {
        // Arrange
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        Mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.CanConnect()).Returns(true);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        Mock.Mock<IReaparrDbContextDatabase>()
            .Setup(x => x.GetPendingMigrations())
            .Returns(["Migration1", "Migration2"]);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok()).Verifiable(Times.Once);
        Mock.Mock<IAuthDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        Mock.Mock<IAuthDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns([]);

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IReaparrDbContextDatabase>().Verify(x => x.Migrate(), Times.Once);
        Mock.Mock<IAuthDbContextDatabase>().Verify(x => x.Migrate(), Times.Never);
    }

    [Fact]
    public void ShouldMigrateAuthDatabase_WhenPendingMigrationsExist()
    {
        // Arrange
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        Mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.CanConnect()).Returns(true);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns([]);
        Mock.Mock<IAuthDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        Mock.Mock<IAuthDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns(["AuthMigration1"]);
        Mock.Mock<IAuthDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok()).Verifiable(Times.Once);

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IReaparrDbContextDatabase>().Verify(x => x.Migrate(), Times.Never);
        Mock.Mock<IAuthDbContextDatabase>().Verify(x => x.Migrate(), Times.Once);
    }

    [Fact]
    public void ShouldSkipMigration_WhenDatabaseIsInMemory()
    {
        // Arrange
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        Mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.CanConnect()).Returns(true);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(true);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns(["Migration1"]);
        Mock.Mock<IAuthDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(true);
        Mock.Mock<IAuthDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns(["AuthMigration1"]);

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IReaparrDbContextDatabase>().Verify(x => x.Migrate(), Times.Never);
        Mock.Mock<IAuthDbContextDatabase>().Verify(x => x.Migrate(), Times.Never);
    }

    [Fact]
    public void ShouldResetDatabase_WhenReaparrMigrationFails()
    {
        // Arrange
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabaseBackupDirectory).Returns(() => "/backup");
        Mock.Mock<IPathProvider>().Setup(x => x.DatabaseFiles).Returns(() => [DatabasePath]);
        Mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        Mock.Mock<IFile>().Setup(x => x.Copy(It.IsAny<string>(), It.IsAny<string>()));
        Mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object);

        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.CanConnect()).Returns(true);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns(["Migration1"]);
        Mock.Mock<IReaparrDbContextDatabase>()
            .SetupSequence(x => x.Migrate())
            .Returns(Result.Fail("Migration failed"))
            .Returns(Result.Ok()); // Second call during database reset
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.CloseConnection());
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.EnsureDeleted()).Returns(Result.Ok(true));
        Mock.Mock<IAuthDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok());
        Mock.Mock<IAuthDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        Mock.Mock<IAuthDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns([]);
        Mock.Mock<IGeneralSettings>().SetupSet(x => x.FirstTimeSetup = true).Verifiable(Times.AtLeastOnce);

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IReaparrDbContextDatabase>().Verify(x => x.Migrate(), Times.Exactly(2)); // Once for a migration attempt and once for a reset
        Mock.Mock<IReaparrDbContextDatabase>().Verify(x => x.EnsureDeleted(), Times.Once);
    }

    [Fact]
    public void ShouldResetDatabase_WhenAuthDatabaseMigrationFails()
    {
        // Arrange
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabaseBackupDirectory).Returns(() => "/backup");
        Mock.Mock<IPathProvider>().Setup(x => x.DatabaseFiles).Returns(() => [DatabasePath]);
        Mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        Mock.Mock<IFile>().Setup(x => x.Copy(It.IsAny<string>(), It.IsAny<string>()));
        Mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object);

        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.CanConnect()).Returns(true);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns([]);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok());
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.CloseConnection());
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.EnsureDeleted()).Returns(Result.Ok(true));

        Mock.Mock<IAuthDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        Mock.Mock<IAuthDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns(["AuthMigration1"]);
        Mock.Mock<IAuthDbContextDatabase>()
            .SetupSequence(x => x.Migrate())
            .Returns(Result.Fail("Auth migration failed"))
            .Returns(Result.Ok()); // Second call during database reset
        Mock.Mock<IGeneralSettings>().SetupSet(x => x.FirstTimeSetup = true).Verifiable(Times.AtLeastOnce);

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IAuthDbContextDatabase>().Verify(x => x.Migrate(), Times.Exactly(2)); // Once for migration attempt, once for reset
        Mock.Mock<IReaparrDbContextDatabase>().Verify(x => x.EnsureDeleted(), Times.Once);
    }

    [Fact]
    public void ShouldResetDatabase_WhenMigrationThrowsException()
    {
        // Arrange
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabaseBackupDirectory).Returns(() => "/backup");
        Mock.Mock<IPathProvider>().Setup(x => x.DatabaseFiles).Returns(() => [DatabasePath]);
        Mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        Mock.Mock<IFile>().Setup(x => x.Copy(It.IsAny<string>(), It.IsAny<string>()));
        Mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object);

        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.CanConnect()).Returns(true);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns(["Migration1"]);
        Mock.Mock<IReaparrDbContextDatabase>()
            .SetupSequence(x => x.Migrate())
            .Throws(new Exception("Migration exception"))
            .Returns(Result.Ok()); // Second call during database reset
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.CloseConnection());
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.EnsureDeleted()).Returns(Result.Ok(true));
        Mock.Mock<IAuthDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok());
        Mock.Mock<IAuthDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        Mock.Mock<IAuthDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns([]);
        Mock.Mock<IGeneralSettings>().SetupSet(x => x.FirstTimeSetup = true).Verifiable(Times.Once);

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IReaparrDbContextDatabase>().Verify(x => x.EnsureDeleted(), Times.Once);
    }

    [Fact]
    public void ShouldFailBackup_WhenBackupDirectoryCannotBeCreated()
    {
        // Arrange
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabaseBackupDirectory).Returns(() => "/backup");
        Mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        Mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Throws(new Exception("Directory creation failed"));
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.CanConnect()).Returns(false);

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public void ShouldHandlePartialBackup_WhenSomeDatabaseFilesAreMissing()
    {
        // Arrange
        List<string> dbFiles = [DatabasePath, DatabasePath + "-shm", DatabasePath + "-wal"];
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabaseBackupDirectory).Returns(() => "/backup");
        Mock.Mock<IPathProvider>().Setup(x => x.DatabaseFiles).Returns(() => dbFiles);
        Mock.Mock<IFile>().Setup(x => x.Exists(DatabasePath)).Returns(true);
        Mock.Mock<IFile>().Setup(x => x.Exists(DatabasePath + "-shm")).Returns(false);
        Mock.Mock<IFile>().Setup(x => x.Exists(DatabasePath + "-wal")).Returns(true);
        Mock.Mock<IFile>().Setup(x => x.Copy(It.IsAny<string>(), It.IsAny<string>()));
        Mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object);

        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.CanConnect()).Returns(false);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.CloseConnection());
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.EnsureDeleted()).Returns(Result.Ok(true));
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok());
        Mock.Mock<IAuthDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok());
        Mock.Mock<IGeneralSettings>().SetupSet(x => x.FirstTimeSetup = true).Verifiable(Times.Once);

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IFile>().Verify(x => x.Copy(DatabasePath, It.IsAny<string>()), Times.Once);
        Mock.Mock<IFile>().Verify(x => x.Copy(DatabasePath + "-wal", It.IsAny<string>()), Times.Once);
        Mock.Mock<IFile>().Verify(x => x.Copy(DatabasePath + "-shm", It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ShouldFailReset_WhenDatabaseDeletionFails()
    {
        // Arrange
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabaseBackupDirectory).Returns(() => "/backup");
        Mock.Mock<IPathProvider>().Setup(x => x.DatabaseFiles).Returns(() => [DatabasePath]);
        Mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        Mock.Mock<IFile>().Setup(x => x.Copy(It.IsAny<string>(), It.IsAny<string>()));
        Mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object);

        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.CanConnect()).Returns(false);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.CloseConnection());
        Mock.Mock<IReaparrDbContextDatabase>()
            .Setup(x => x.EnsureDeleted())
            .Returns(Result.Fail("Database deletion failed"));

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsFailed.ShouldBeTrue();
        Mock.Mock<IReaparrDbContextDatabase>().Verify(x => x.EnsureDeleted(), Times.Once);
        Mock.Mock<IReaparrDbContextDatabase>().Verify(x => x.Migrate(), Times.Never); // Should not attempt to recreate if deletion failed
    }

    [Fact]
    public void ShouldFailReset_WhenDatabaseCreationFailsAfterReset()
    {
        // Arrange
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabaseBackupDirectory).Returns(() => "/backup");
        Mock.Mock<IPathProvider>().Setup(x => x.DatabaseFiles).Returns(() => [DatabasePath]);
        Mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        Mock.Mock<IFile>().Setup(x => x.Copy(It.IsAny<string>(), It.IsAny<string>()));
        Mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object);

        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.CanConnect()).Returns(false);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.CloseConnection());
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.EnsureDeleted()).Returns(Result.Ok(true));
        Mock.Mock<IReaparrDbContextDatabase>()
            .Setup(x => x.Migrate())
            .Throws(new Exception("Database creation failed"));

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsFailed.ShouldBeTrue();
        Mock.Mock<IReaparrDbContextDatabase>().Verify(x => x.EnsureDeleted(), Times.Once);
        Mock.Mock<IReaparrDbContextDatabase>().Verify(x => x.Migrate(), Times.Once);
    }

    [Fact]
    public void ShouldHandleExceptionInResetDatabase_AndReturnFailure()
    {
        // Arrange
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.CanConnect()).Returns(false);
        Mock.Mock<IReaparrDbContextDatabase>()
            .Setup(x => x.CloseConnection())
            .Throws(new Exception("Connection close failed"));
        Mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public void ShouldSetFirstTimeSetupToTrue_WhenDatabaseIsReset()
    {
        // Arrange
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabaseBackupDirectory).Returns(() => "/backup");
        Mock.Mock<IPathProvider>().Setup(x => x.DatabaseFiles).Returns(() => [DatabasePath]);
        Mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        Mock.Mock<IFile>().Setup(x => x.Copy(It.IsAny<string>(), It.IsAny<string>()));
        Mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object);

        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.CanConnect()).Returns(false);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.CloseConnection());
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.EnsureDeleted()).Returns(Result.Ok(true));
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok());
        Mock.Mock<IAuthDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok());

        var generalSettingsMock = Mock.Mock<IGeneralSettings>();
        generalSettingsMock.SetupProperty(x => x.FirstTimeSetup);

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        generalSettingsMock.VerifySet(x => x.FirstTimeSetup = true, Times.Once);
    }

    [Fact]
    public void ShouldSkipBackup_WhenDatabaseDoesNotExistDuringReset()
    {
        // Arrange
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        Mock.Mock<IFile>().Setup(x => x.Exists(DatabasePath)).Returns(false); // Database doesn't exist for backup
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.CanConnect()).Returns(false);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.CloseConnection());
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.EnsureDeleted()).Returns(Result.Ok(true));
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok());
        Mock.Mock<IAuthDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok());

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IDirectory>().Verify(x => x.CreateDirectory(It.IsAny<string>()), Times.Never);
        Mock.Mock<IFile>().Verify(x => x.Copy(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ShouldFailAuthDatabaseCreation_WhenExceptionIsThrown()
    {
        // Arrange
        Mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        Mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
        Mock.Mock<IReaparrDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok());
        Mock.Mock<IAuthDbContextDatabase>()
            .Setup(x => x.Migrate())
            .Throws(new Exception("Auth database creation failed"));

        // Act
        var result = Sut.Setup();

        // Assert
        result.IsFailed.ShouldBeTrue();
        Mock.Mock<IReaparrDbContextDatabase>().Verify(x => x.Migrate(), Times.Once);
        Mock.Mock<IAuthDbContextDatabase>().Verify(x => x.Migrate(), Times.Once);
    }
}
