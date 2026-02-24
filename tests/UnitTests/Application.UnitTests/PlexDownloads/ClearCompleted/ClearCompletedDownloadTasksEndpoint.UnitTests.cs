using Microsoft.EntityFrameworkCore;
using Reaparr.Application;
using Reaparr.Application.Contracts;

namespace Reaparr.Application.UnitTests;

public class ClearCompletedDownloadTasksEndpointUnitTests : BaseUnitTest<ClearCompletedDownloadTasksEndpoint>
{
    public ClearCompletedDownloadTasksEndpointUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldRemoveOnlySpecifiedCompletedDownloadTasks_WhenClearCompletedEndpointIsCalledWithGuidList()
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

        // Set download tasks to completed
        var dbContext = IDbContext;
        var downloadTasks = await dbContext
            .DownloadTaskMovie.AsTracking()
            .Include(x => x.Children)
            .ToListAsync(CancellationToken);

        downloadTasks.SetDownloadStatus(DownloadStatus.Completed);
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<ClearCompletedDownloadTasksCommand>(), It.IsAny<CancellationToken>()))
            .Returns(
                (ClearCompletedDownloadTasksCommand command, CancellationToken ct) =>
                    new ClearCompletedDownloadTasksCommandHandler(dbContext).ExecuteAsync(command, ct)
            );

        // Act
        var ep = SetupEndpointUnitTest<ClearCompletedDownloadTasksEndpoint>();
        await ep.HandleAsync(downloadTasks.Select(x => x.Id).Take(5).ToList(), CancellationToken);
        var result = ep.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();

        var downloadTasksDb = await dbContext.DownloadTaskMovie.ToListAsync(CancellationToken);
        var downloadTasksFileDb = await dbContext.DownloadTaskMovieFile.ToListAsync(CancellationToken);
        downloadTasksDb.Count.ShouldBe(5);
        downloadTasksFileDb.Count.ShouldBe(5);
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<ClearCompletedDownloadTasksCommand>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
    }

    [Fact]
    public async Task ShouldRemoveAllCompletedDownloadTasks_WhenClearCompletedEndpointIsCalled()
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

        // Set download tasks to completed
        var dbContext = IDbContext;
        var downloadTasks = await dbContext
            .DownloadTaskMovie.AsTracking()
            .Include(x => x.Children)
            .ToListAsync(CancellationToken);

        downloadTasks.SetDownloadStatus(DownloadStatus.Completed);
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<ClearCompletedDownloadTasksCommand>(), It.IsAny<CancellationToken>()))
            .Returns(
                (ClearCompletedDownloadTasksCommand command, CancellationToken ct) =>
                    new ClearCompletedDownloadTasksCommandHandler(dbContext).ExecuteAsync(command, ct)
            );

        // Act
        var ep = SetupEndpointUnitTest<ClearCompletedDownloadTasksEndpoint>();
        await ep.HandleAsync([], CancellationToken);
        var result = ep.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        var downloadTasksDb = await dbContext.DownloadTaskMovie.ToListAsync(CancellationToken);
        var downloadTasksFileDb = await dbContext.DownloadTaskMovieFile.ToListAsync(CancellationToken);
        downloadTasksDb.ShouldBeEmpty();
        downloadTasksFileDb.ShouldBeEmpty();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<ClearCompletedDownloadTasksCommand>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
    }
}
