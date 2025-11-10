using Microsoft.EntityFrameworkCore;
using Reaparr.Data.Contracts;

namespace Reaparr.Application.UnitTests;

public class RefreshPlexMovieLibraryCommandUnitTests : BaseUnitTest<RefreshPlexMovieLibraryCommandHandler>
{
    public RefreshPlexMovieLibraryCommandUnitTests(ITestOutputHelper output)
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
        Mock.Mock<IRefreshLibraryProgressReporter>()
            .Setup(x => x.SendProgress(It.IsAny<RefreshLibraryProgressUpdate>()))
            .Returns(Task.CompletedTask);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<SyncPlexMoviesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new CrudMoviesReport()));

        // Act
        var result = await Sut.ExecuteAsync(
            new RefreshPlexMovieLibraryCommand(new InsertMediaMetaDataCommandResponse(testLibrary), _ => { }),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var updatedLibrary = await IDbContext.PlexLibraries.GetAsync(testLibrary.Id, CancellationToken);
        updatedLibrary.ShouldNotBeNull();
        updatedLibrary.SyncedAt.ShouldNotBeNull();
        Mock.Mock<IRefreshLibraryProgressReporter>()
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

        Mock.Mock<IRefreshLibraryProgressReporter>()
            .Setup(x => x.SendProgress(It.IsAny<RefreshLibraryProgressUpdate>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await Sut.ExecuteAsync(
            new RefreshPlexMovieLibraryCommand(new InsertMediaMetaDataCommandResponse(testLibrary), _ => { }),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var updatedLibrary = await IDbContext.PlexLibraries.GetAsync(testLibrary.Id, CancellationToken);
        updatedLibrary.ShouldNotBeNull();
        updatedLibrary.SyncedAt.ShouldNotBeNull();

        Mock.Mock<IRefreshLibraryProgressReporter>()
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

        Mock.Mock<IRefreshLibraryProgressReporter>()
            .Setup(x => x.SendProgress(It.IsAny<RefreshLibraryProgressUpdate>()))
            .Returns(Task.CompletedTask);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<SyncPlexMoviesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new CrudMoviesReport()));

        // Act
        var result = await Sut.ExecuteAsync(
            new RefreshPlexMovieLibraryCommand(new InsertMediaMetaDataCommandResponse(testLibrary), _ => { }),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var updatedLibrary = await IDbContext.PlexLibraries.GetAsync(testLibrary.Id, CancellationToken);
        updatedLibrary.ShouldNotBeNull();
        updatedLibrary.SyncedAt.ShouldNotBeNull();

        Mock.Mock<IRefreshLibraryProgressReporter>()
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
        Mock.Mock<IRefreshLibraryProgressReporter>()
            .Setup(x => x.SendProgress(It.IsAny<RefreshLibraryProgressUpdate>()))
            .Returns(Task.CompletedTask);
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<SyncPlexMoviesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Failed to sync movies"));

        // Act
        var result = await Sut.ExecuteAsync(
            new RefreshPlexMovieLibraryCommand(new InsertMediaMetaDataCommandResponse(testLibrary), _ => { }),
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        Mock.Mock<IRefreshLibraryProgressReporter>()
            .Verify(x => x.SendProgress(It.IsAny<RefreshLibraryProgressUpdate>()), Times.Once);
    }
}
