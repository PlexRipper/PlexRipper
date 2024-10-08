using Data.Contracts;
using Environment;
using FileSystem.Contracts;
using PlexRipper.Data;

namespace Domain.UnitTests;

public class PlexRipperDbContextManager_UnitTests : BaseUnitTest<PlexRipperDbContextManager>
{
    private string DatabasePath => "/Config/" + PathProvider.DatabaseName;

    public PlexRipperDbContextManager_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldConnectToDatabaseAndCheckToMigrate_WhenDatabaseAlreadyExists()
    {
        // Arrange

        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        mock.Mock<IFileSystem>().Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CanConnect()).Returns(true);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.IsInMemory()).Returns(false);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.GetPendingMigrations()).Returns([]);

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.Mock<IPlexRipperDbContextDatabase>().Verify(x => x.CanConnect(), Times.Once);
        mock.Mock<IPlexRipperDbContextDatabase>().Verify(x => x.GetPendingMigrations(), Times.Once);
        mock.Mock<IPlexRipperDbContextDatabase>().Verify(x => x.Migrate(), Times.Never); // No migrations to apply
    }

    [Fact]
    public void ShouldCreateDatabase_WhenDatabaseDoesNotExist()
    {
        // Arrange
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        mock.Mock<IFileSystem>().Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok());

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.Mock<IPlexRipperDbContextDatabase>().Verify(x => x.Migrate(), Times.Once); // Database creation involves migration
        mock.Mock<IFileSystem>().Verify(x => x.FileExists(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void ShouldLogWarning_WhenDatabaseDoesNotExist()
    {
        // Arrange
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        mock.Mock<IFileSystem>().Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok());

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
        mock.Mock<IFileSystem>().Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.Migrate()).Throws(new Exception("Test Exception"));

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
        mock.Mock<IFileSystem>().Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);
        mock.Mock<IFileSystem>().Setup(x => x.Copy(It.IsAny<string>(), It.IsAny<string>())).Returns(Result.Ok());
        mock.Mock<IDirectorySystem>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Result<DirectoryInfo>());
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok());
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CanConnect()).Returns(false);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.EnsureDeleted()).Returns(Result.Ok(true));
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CloseConnection());

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var mockDbContext = mock.Mock<IPlexRipperDbContextDatabase>();
        mock.Mock<IFileSystem>().Verify(x => x.Copy(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(3));
        mockDbContext.Verify(x => x.CanConnect(), Times.Once);
        mockDbContext.Verify(x => x.EnsureDeleted(), Times.Once); // Database is reset
        mockDbContext.Verify(x => x.Migrate(), Times.Once); // Database is recreated after reset
    }
}
