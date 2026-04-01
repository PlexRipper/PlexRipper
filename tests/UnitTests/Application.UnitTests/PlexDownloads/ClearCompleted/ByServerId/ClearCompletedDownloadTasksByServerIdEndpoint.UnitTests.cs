namespace Reaparr.Application.UnitTests;

public class ClearCompletedDownloadTasksByServerIdEndpointUnitTests
    : BaseUnitTest<ClearCompletedDownloadTasksByServerIdEndpoint>
{
    [Test]
    public async Task ShouldRemoveAllCompletedDownloadTasksForServer_WhenClearCompletedByServerIdEndpointIsCalled()
    {
        // Arrange
        await SetupDatabase(
            34036,
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
            .Setup(x => x.Send(It.IsAny<ClearCompletedDownloadTasksByServerIdCommand>(), It.IsAny<CancellationToken>()))
            .Returns(
                (ClearCompletedDownloadTasksByServerIdCommand command, CancellationToken ct) =>
                    new ClearCompletedDownloadTasksByServerIdCommandHandler(dbContext).ExecuteAsync(command, ct)
            );

        // Act
        var ep = SetupEndpointUnitTest<ClearCompletedDownloadTasksByServerIdEndpoint>();
        var request = new ClearCompletedDownloadTasksByServerIdEndpointRequest
        {
            PlexServerId = downloadTasks[0].PlexServerId,
        };
        await ep.HandleAsync(request, CancellationToken);
        var result = ep.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        (await dbContext.DownloadTaskMovie.ToListAsync(CancellationToken)).ShouldBeEmpty();
        (await dbContext.DownloadTaskMovieFile.ToListAsync(CancellationToken)).ShouldBeEmpty();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<ClearCompletedDownloadTasksByServerIdCommand>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
    }

    [Test]
    public async Task ShouldOnlyClearCompletedTasksForTargetServer_WhenMultipleServersExist()
    {
        // Arrange
        await SetupDatabase(
            71124,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexMovieLibraryCount = 1;
                config.MovieDownloadTasksCount = 5;
            }
        );

        var dbContext = IDbContext;
        var allDownloadTasks = await dbContext
            .DownloadTaskMovie.AsTracking()
            .Include(x => x.Children)
            .ToListAsync(CancellationToken);

        allDownloadTasks.SetDownloadStatus(DownloadStatus.Completed);
        await dbContext.SaveChangesAsync(CancellationToken);

        var targetServerId = allDownloadTasks[0].PlexServerId;
        var otherServerTaskCount = allDownloadTasks.Count(x => x.PlexServerId != targetServerId);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<ClearCompletedDownloadTasksByServerIdCommand>(), It.IsAny<CancellationToken>()))
            .Returns(
                (ClearCompletedDownloadTasksByServerIdCommand command, CancellationToken ct) =>
                    new ClearCompletedDownloadTasksByServerIdCommandHandler(dbContext).ExecuteAsync(command, ct)
            );

        // Act
        var ep = SetupEndpointUnitTest<ClearCompletedDownloadTasksByServerIdEndpoint>();
        var request = new ClearCompletedDownloadTasksByServerIdEndpointRequest { PlexServerId = targetServerId };
        await ep.HandleAsync(request, CancellationToken);
        var result = ep.Response as ResultDTO<CountResponseDTO>;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBeGreaterThan(0);
        var remaining = await dbContext.DownloadTaskMovie.ToListAsync(CancellationToken);
        remaining.Count.ShouldBe(otherServerTaskCount);
        remaining.ShouldAllBe(x => x.PlexServerId != targetServerId);
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.Is<ClearCompletedDownloadTasksByServerIdCommand>(cmd => cmd.PlexServerId == targetServerId),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
    }
}
