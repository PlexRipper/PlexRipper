using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application.UnitTests;

public class RefreshPlexMovieLibraryCommand_UnitTests : BaseUnitTest<RefreshPlexMovieLibraryCommandHandler>
{
    public RefreshPlexMovieLibraryCommand_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldSuccessfullyRefreshLibraryAndUpdateSyncedAt_WhenMoviesExist()
    {
        // Arrange
        await SetupDatabase(
            8423,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexTvShowLibraryCount = 1;
                config.MovieCount = 3;
            }
        );
        var testLibrary = IDbContext.PlexLibraries.Include(x => x.Movies).First();
        mock.Mock<IRefreshLibraryProgressReporter>()
            .Setup(x => x.SendProgress(It.IsAny<RefreshLibraryProgressUpdate>()));

        mock.Mock<IMediator>()
            .Setup(x => x.Send(It.IsAny<SyncPlexMoviesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new CrudMoviesReport()));

        // Act
        var result = await _sut.ExecuteAsync(
            new RefreshPlexMovieLibraryCommand(testLibrary, _ => { }),
            CancellationToken.None
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var updatedLibrary = await IDbContext.PlexLibraries.FindAsync(testLibrary.Id);
        updatedLibrary.ShouldNotBeNull();
        updatedLibrary.SyncedAt.ShouldNotBeNull();
        mock.Mock<IRefreshLibraryProgressReporter>()
            .Verify(x => x.SendProgress(It.IsAny<RefreshLibraryProgressUpdate>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ShouldStillUpdateLibraryAndLogWarning_WhenNoMoviesExist()
    {
        // Arrange
        await SetupDatabase(
            8424,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexTvShowLibraryCount = 1;
                config.MovieCount = 0;
            }
        );
        var testLibrary = IDbContext.PlexLibraries.First();

        mock.Mock<IRefreshLibraryProgressReporter>()
            .Setup(x => x.SendProgress(It.IsAny<RefreshLibraryProgressUpdate>()));

        // Act
        var result = await _sut.ExecuteAsync(
            new RefreshPlexMovieLibraryCommand(testLibrary, _ => { }),
            CancellationToken.None
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var updatedLibrary = await IDbContext.PlexLibraries.FindAsync(testLibrary.Id);
        updatedLibrary.ShouldNotBeNull();
        updatedLibrary.SyncedAt.ShouldNotBeNull();

        mock.Mock<IRefreshLibraryProgressReporter>()
            .Verify(x => x.SendProgress(It.IsAny<RefreshLibraryProgressUpdate>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ShouldUpdateLibraryAndLogError_WhenMoviesExistWithZeroMediaSize()
    {
        // Arrange
        await SetupDatabase(
            8425,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexTvShowLibraryCount = 1;
                config.MovieCount = 3;
            }
        );
        var testLibrary = IDbContext.PlexLibraries.Include(x => x.Movies).First();
        foreach (var movie in testLibrary.Movies)
        {
            movie.MediaSize = 0;
        }

        mock.Mock<IRefreshLibraryProgressReporter>()
            .Setup(x => x.SendProgress(It.IsAny<RefreshLibraryProgressUpdate>()));

        mock.Mock<IMediator>()
            .Setup(x => x.Send(It.IsAny<SyncPlexMoviesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new CrudMoviesReport()));

        // Act
        var result = await _sut.ExecuteAsync(
            new RefreshPlexMovieLibraryCommand(testLibrary, _ => { }),
            CancellationToken.None
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var updatedLibrary = await IDbContext.PlexLibraries.FindAsync(testLibrary.Id);
        updatedLibrary.ShouldNotBeNull();
        updatedLibrary.SyncedAt.ShouldNotBeNull();

        mock.Mock<IRefreshLibraryProgressReporter>()
            .Verify(x => x.SendProgress(It.IsAny<RefreshLibraryProgressUpdate>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenSyncPlexMoviesCommandFails()
    {
        // Arrange
        await SetupDatabase(
            8426,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexTvShowLibraryCount = 1;
                config.MovieCount = 5;
            }
        );
        var testLibrary = IDbContext.PlexLibraries.Include(x => x.Movies).First();
        mock.Mock<IRefreshLibraryProgressReporter>()
            .Setup(x => x.SendProgress(It.IsAny<RefreshLibraryProgressUpdate>()));
        mock.Mock<IMediator>()
            .Setup(x => x.Send(It.IsAny<SyncPlexMoviesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Failed to sync movies"));

        // Act
        var result = await _sut.ExecuteAsync(
            new RefreshPlexMovieLibraryCommand(testLibrary, _ => { }),
            CancellationToken.None
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        mock.Mock<IRefreshLibraryProgressReporter>()
            .Verify(x => x.SendProgress(It.IsAny<RefreshLibraryProgressUpdate>()), Times.Once);
    }
}
