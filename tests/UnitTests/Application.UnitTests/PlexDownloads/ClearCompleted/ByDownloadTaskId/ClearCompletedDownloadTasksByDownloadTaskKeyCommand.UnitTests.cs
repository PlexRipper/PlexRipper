using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application.UnitTests;

public class ClearCompletedDownloadTasksByDownloadTaskKeyCommandUnitTests
    : BaseUnitTest<ClearCompletedDownloadTasksByDownloadTaskKeyCommandHandler>
{
    public ClearCompletedDownloadTasksByDownloadTaskKeyCommandUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnSuccessWithZeroCount_WhenNoneOfTheKeysAreCompleted()
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
        var nonExistentKeys = new List<DownloadTaskKey>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Type = DownloadTaskType.Movie,
                PlexServerId = 1,
                PlexLibraryId = 1,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Type = DownloadTaskType.Movie,
                PlexServerId = 1,
                PlexLibraryId = 1,
            },
        };

        // Act
        var result = await Sut.ExecuteAsync(
            new ClearCompletedDownloadTasksByDownloadTaskKeyCommand(nonExistentKeys),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(0);
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<DeleteDownloadTasksByKeyCommand>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
    }

    [Fact]
    public async Task ShouldDispatchDeleteCommand_WhenCompletedMovieTaskKeysAreGiven()
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

        var taskKeys = await dbContext.DownloadTaskMovie.ProjectToKey().ToListAsync(CancellationToken);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DeleteDownloadTasksByKeyCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(
            new ClearCompletedDownloadTasksByDownloadTaskKeyCommand(taskKeys),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeGreaterThan(0);
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.Is<DeleteDownloadTasksByKeyCommand>(cmd =>
                            taskKeys.TrueForAll(k => cmd.Keys.Any(ck => ck.Id == k.Id))
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
    }

    [Fact]
    public async Task ShouldNotDispatchDeleteCommand_WhenTasksAreNotCompleted()
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

        var taskKeys = await dbContext.DownloadTaskMovie.ProjectToKey().ToListAsync(CancellationToken);

        // Act
        var result = await Sut.ExecuteAsync(
            new ClearCompletedDownloadTasksByDownloadTaskKeyCommand(taskKeys),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(0);
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<DeleteDownloadTasksByKeyCommand>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
    }

    [Fact]
    public async Task ShouldDispatchOnlyCompletedKeys_WhenPartialKeyListIsGiven()
    {
        // Arrange — 10 tasks all Completed; only 3 keys passed to the command.
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

        var toDeleteKeys = await dbContext.DownloadTaskMovie.ProjectToKey().Take(3).ToListAsync(CancellationToken);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DeleteDownloadTasksByKeyCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(
            new ClearCompletedDownloadTasksByDownloadTaskKeyCommand(toDeleteKeys),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeGreaterThan(0);
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.Is<DeleteDownloadTasksByKeyCommand>(cmd =>
                            toDeleteKeys.TrueForAll(k => cmd.Keys.Any(ck => ck.Id == k.Id))
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
    }

    [Fact]
    public async Task ShouldDispatchDeleteCommand_WhenEpisodeFileKeyIsCompleted()
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

        var episodeFileKey = await dbContext
            .DownloadTaskTvShowEpisodeFile.ProjectToKey()
            .SingleAsync(CancellationToken);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DeleteDownloadTasksByKeyCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(
            new ClearCompletedDownloadTasksByDownloadTaskKeyCommand([episodeFileKey]),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeGreaterThan(0);
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.Is<DeleteDownloadTasksByKeyCommand>(cmd => cmd.Keys.Any(k => k.Id == episodeFileKey.Id)),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
    }
}
