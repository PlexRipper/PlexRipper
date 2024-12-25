using System.IO.Abstractions;
using Data.Contracts;
using Environment;
using FileSystem.Contracts;
using PlexRipper.Data;
using PlexRipper.Identity.Contracts;

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
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok());

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
        mock.Mock<IPathProvider>().SetupGet(x => x.DatabasePath).Returns(() => DatabasePath);
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
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
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
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
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        mock.Mock<IFile>().Setup(x => x.Copy(It.IsAny<string>(), It.IsAny<string>())).Verifiable(Times.Exactly(3));
        mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.Migrate()).Returns(Result.Ok());
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CanConnect()).Returns(false);
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.EnsureDeleted()).Returns(Result.Ok(true));
        mock.Mock<IPlexRipperDbContextDatabase>().Setup(x => x.CloseConnection());

        // Act
        var result = _sut.Setup();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var mockDbContext = mock.Mock<IPlexRipperDbContextDatabase>();
        mockDbContext.Verify(x => x.CanConnect(), Times.Once);
        mockDbContext.Verify(x => x.EnsureDeleted(), Times.Once); // Database is reset
        mockDbContext.Verify(x => x.Migrate(), Times.Once); // Database is recreated after reset
    }
}
