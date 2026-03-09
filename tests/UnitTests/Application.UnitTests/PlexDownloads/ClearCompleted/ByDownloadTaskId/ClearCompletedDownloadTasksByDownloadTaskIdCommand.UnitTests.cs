using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;

namespace Reaparr.Application.UnitTests;

public class ClearCompletedDownloadTasksByDownloadTaskIdCommandUnitTests
    : BaseUnitTest<ClearCompletedDownloadTasksByDownloadTaskIdCommandHandler>
{
    public ClearCompletedDownloadTasksByDownloadTaskIdCommandUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnSuccessWithZeroCount_WhenPassedIdsDoNotExistInDatabase()
    {
        // Arrange
        await SetupDatabase(
            55001,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieDownloadTasksCount = 5;
            }
        );

        // Act
        var result = await Sut.ExecuteAsync(
            new ClearCompletedDownloadTasksByDownloadTaskIdCommand([Guid.NewGuid(), Guid.NewGuid()]),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(0);
        (await IDbContext.DownloadTaskMovie.CountAsync(CancellationToken)).ShouldBe(5);
    }

    [Fact]
    public async Task ShouldDeleteCompletedMovieTasks_WhenAllCompletedIdsAreGiven()
    {
        // Arrange
        await SetupDatabase(
            55002,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieDownloadTasksCount = 5;
            }
        );

        var dbContext = IDbContext;
        var downloadTasks = await dbContext
            .DownloadTaskMovie.AsTracking()
            .Include(x => x.Children)
            .ToListAsync(CancellationToken);

        downloadTasks.SetDownloadStatus(DownloadStatus.Completed);
        await dbContext.SaveChangesAsync(CancellationToken);

        // Act
        var result = await Sut.ExecuteAsync(
            new ClearCompletedDownloadTasksByDownloadTaskIdCommand(downloadTasks.Select(x => x.Id).ToList()),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeGreaterThan(0);
        (await dbContext.DownloadTaskMovie.CountAsync(CancellationToken)).ShouldBe(0);
        (await dbContext.DownloadTaskMovieFile.CountAsync(CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task ShouldNotDeleteTasks_WhenTasksAreNotCompleted()
    {
        // Arrange
        await SetupDatabase(
            55003,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieDownloadTasksCount = 5;
            }
        );

        var dbContext = IDbContext;
        var downloadTasks = await dbContext
            .DownloadTaskMovie.AsTracking()
            .Include(x => x.Children)
            .ToListAsync(CancellationToken);

        downloadTasks.SetDownloadStatus(DownloadStatus.Downloading);
        await dbContext.SaveChangesAsync(CancellationToken);

        // Act
        var result = await Sut.ExecuteAsync(
            new ClearCompletedDownloadTasksByDownloadTaskIdCommand(downloadTasks.Select(x => x.Id).ToList()),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(0);
        (await dbContext.DownloadTaskMovie.CountAsync(CancellationToken)).ShouldBe(5);
    }

    [Fact]
    public async Task ShouldDeleteOnlySpecifiedTasks_WhenPartialIdListIsGiven()
    {
        // Arrange
        await SetupDatabase(
            55004,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieDownloadTasksCount = 10;
            }
        );

        var dbContext = IDbContext;
        var downloadTasks = await dbContext
            .DownloadTaskMovie.AsTracking()
            .Include(x => x.Children)
            .ToListAsync(CancellationToken);

        downloadTasks.SetDownloadStatus(DownloadStatus.Completed);
        await dbContext.SaveChangesAsync(CancellationToken);

        var toDeleteIds = downloadTasks.Select(x => x.Id).Take(3).ToList();

        // Act
        var result = await Sut.ExecuteAsync(
            new ClearCompletedDownloadTasksByDownloadTaskIdCommand(toDeleteIds),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeGreaterThan(0);
        (await dbContext.DownloadTaskMovie.CountAsync(CancellationToken)).ShouldBe(7);
        (await dbContext.DownloadTaskMovieFile.CountAsync(CancellationToken)).ShouldBe(7);
    }

    [Fact]
    public async Task ShouldDeleteOrphanedParentTasks_WhenLastEpisodeFileIsCleared()
    {
        // Arrange
        await SetupDatabase(
            55005,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexTvShowLibraryCount = 1;
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var tvShowTasks = await dbContext
            .DownloadTaskTvShow.AsTracking()
            .Include(x => x.Children)
                .ThenInclude(x => x.Children)
                    .ThenInclude(x => x.Children)
            .ToListAsync(CancellationToken);

        tvShowTasks.SetDownloadStatus(DownloadStatus.Completed);
        await dbContext.SaveChangesAsync(CancellationToken);

        var episodeFileId = tvShowTasks
            .SelectMany(x => x.Children)
            .SelectMany(x => x.Children)
            .SelectMany(x => x.Children)
            .Select(x => x.Id)
            .Single();

        // Act
        var result = await Sut.ExecuteAsync(
            new ClearCompletedDownloadTasksByDownloadTaskIdCommand([episodeFileId]),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeGreaterThan(0);
        (await dbContext.DownloadTaskTvShow.CountAsync(CancellationToken)).ShouldBe(0);
        (await dbContext.DownloadTaskTvShowSeason.CountAsync(CancellationToken)).ShouldBe(0);
        (await dbContext.DownloadTaskTvShowEpisode.CountAsync(CancellationToken)).ShouldBe(0);
        (await dbContext.DownloadTaskTvShowEpisodeFile.CountAsync(CancellationToken)).ShouldBe(0);
    }
}
