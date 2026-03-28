using Microsoft.EntityFrameworkCore;
using Reaparr.Application;
using Reaparr.Application.Contracts;

namespace Reaparr.Application.UnitTests;

public class ClearCompletedDownloadTasksByDownloadTaskIdEndpointUnitTests
    : BaseUnitTest<ClearCompletedDownloadTasksByDownloadTaskIdEndpoint>
{
    public ClearCompletedDownloadTasksByDownloadTaskIdEndpointUnitTests()
        : base() { }

    [Test]
    public async Task ShouldRemoveOnlySpecifiedCompletedDownloadTasks_WhenCalledWithGuidList()
    {
        // Arrange
        await SetupDatabase(
            213132,
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

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DeleteDownloadTasksByKeyCommand>(), It.IsAny<CancellationToken>()))
            .Returns(
                (DeleteDownloadTasksByKeyCommand cmd, CancellationToken ct) =>
                    new DeleteDownloadTasksByKeyCommandHandler(dbContext).ExecuteAsync(cmd, ct)
            );
        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(It.IsAny<ClearCompletedDownloadTasksByDownloadTaskKeyCommand>(), It.IsAny<CancellationToken>())
            )
            .Returns(
                (ClearCompletedDownloadTasksByDownloadTaskKeyCommand command, CancellationToken ct) =>
                    new ClearCompletedDownloadTasksByDownloadTaskKeyCommandHandler(
                        dbContext,
                        Mock.Mock<ICommandExecutor>().Object
                    ).ExecuteAsync(command, ct)
            );

        // Act
        var ep = SetupEndpointUnitTest<ClearCompletedDownloadTasksByDownloadTaskIdEndpoint>();
        await ep.HandleAsync(downloadTasks.Select(x => x.Id).Take(5).ToList(), CancellationToken);
        var result = ep.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        (await dbContext.DownloadTaskMovie.ToListAsync(CancellationToken)).Count.ShouldBe(5);
        (await dbContext.DownloadTaskMovieFile.ToListAsync(CancellationToken)).Count.ShouldBe(5);
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.IsAny<ClearCompletedDownloadTasksByDownloadTaskKeyCommand>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
    }

    [Test]
    public async Task ShouldNotRemoveDownloadTasks_WhenTasksAreNotCompleted()
    {
        // Arrange
        await SetupDatabase(
            88341,
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

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DeleteDownloadTasksByKeyCommand>(), It.IsAny<CancellationToken>()))
            .Returns(
                (DeleteDownloadTasksByKeyCommand cmd, CancellationToken ct) =>
                    new DeleteDownloadTasksByKeyCommandHandler(dbContext).ExecuteAsync(cmd, ct)
            );
        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(It.IsAny<ClearCompletedDownloadTasksByDownloadTaskKeyCommand>(), It.IsAny<CancellationToken>())
            )
            .Returns(
                (ClearCompletedDownloadTasksByDownloadTaskKeyCommand command, CancellationToken ct) =>
                    new ClearCompletedDownloadTasksByDownloadTaskKeyCommandHandler(
                        dbContext,
                        Mock.Mock<ICommandExecutor>().Object
                    ).ExecuteAsync(command, ct)
            );

        // Act
        var ep = SetupEndpointUnitTest<ClearCompletedDownloadTasksByDownloadTaskIdEndpoint>();
        await ep.HandleAsync(downloadTasks.Select(x => x.Id).ToList(), CancellationToken);
        var result = ep.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        (await dbContext.DownloadTaskMovie.ToListAsync(CancellationToken)).Count.ShouldBe(5);
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.IsAny<ClearCompletedDownloadTasksByDownloadTaskKeyCommand>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
    }

    [Test]
    public async Task ShouldRemoveOrphanedTvShowParents_WhenLastCompletedEpisodeFileIsClearedById()
    {
        // Arrange
        await SetupDatabase(
            66812,
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
        var tvShowDownloadTasks = await dbContext
            .DownloadTaskTvShow.AsTracking()
            .Include(x => x.Children)
                .ThenInclude(x => x.Children)
                    .ThenInclude(x => x.Children)
            .ToListAsync(CancellationToken);

        tvShowDownloadTasks.SetDownloadStatus(DownloadStatus.Completed);
        await dbContext.SaveChangesAsync(CancellationToken);

        var episodeFileId = tvShowDownloadTasks
            .SelectMany(x => x.Children)
            .SelectMany(x => x.Children)
            .SelectMany(x => x.Children)
            .Select(x => x.Id)
            .Single();

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DeleteDownloadTasksByKeyCommand>(), It.IsAny<CancellationToken>()))
            .Returns(
                (DeleteDownloadTasksByKeyCommand cmd, CancellationToken ct) =>
                    new DeleteDownloadTasksByKeyCommandHandler(dbContext).ExecuteAsync(cmd, ct)
            );
        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(It.IsAny<ClearCompletedDownloadTasksByDownloadTaskKeyCommand>(), It.IsAny<CancellationToken>())
            )
            .Returns(
                (ClearCompletedDownloadTasksByDownloadTaskKeyCommand command, CancellationToken ct) =>
                    new ClearCompletedDownloadTasksByDownloadTaskKeyCommandHandler(
                        dbContext,
                        Mock.Mock<ICommandExecutor>().Object
                    ).ExecuteAsync(command, ct)
            );

        // Act
        var ep = SetupEndpointUnitTest<ClearCompletedDownloadTasksByDownloadTaskIdEndpoint>();
        await ep.HandleAsync([episodeFileId], CancellationToken);
        var result = ep.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        (await dbContext.DownloadTaskTvShow.ToListAsync(CancellationToken)).ShouldBeEmpty();
        (await dbContext.DownloadTaskTvShowSeason.ToListAsync(CancellationToken)).ShouldBeEmpty();
        (await dbContext.DownloadTaskTvShowEpisode.ToListAsync(CancellationToken)).ShouldBeEmpty();
        (await dbContext.DownloadTaskTvShowEpisodeFile.ToListAsync(CancellationToken)).ShouldBeEmpty();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.IsAny<ClearCompletedDownloadTasksByDownloadTaskKeyCommand>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
    }
}
