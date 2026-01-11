using Microsoft.EntityFrameworkCore;
using Reaparr.Data.Contracts;

namespace Reaparr.IntegrationTests.FileSystem;

public class MoveDownloadFileSchedulerIntegrationTests : BaseIntegrationTests
{
    public MoveDownloadFileSchedulerIntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldFinishMergingDownloadTaskAsFileTaskJobAndSetToCompleted_WhenDownloadTaskHasFinishedDownloading()
    {
        // Arrange
        using var container = await CreateContainer(
            235689,
            config =>
            {
                config.DatabaseOptions = x =>
                {
                    x.PlexServerCount = 1;
                    x.PlexMovieLibraryCount = 3;
                    x.MovieCount = 1;
                    x.MovieDownloadTasksCount = 1;
                    x.DownloadWorkerTasks = 4;
                    x.DownloadFileSizeInMb = 10;
                };

                config.FileSystemOptions = (system, dbContext) =>
                {
                    var downloadTask = dbContext.DownloadTaskMovieFile.Include(x => x.DownloadWorkerTasks).First();
                    downloadTask.DownloadFilePath.ShouldNotBeNullOrEmpty();

                    system.AddFile(downloadTask.DownloadFilePath, FakeData.GetFileMockData(10, 4));
                };
            }
        );
        var dbContext = container.DbContext;
        var downloadTasks = dbContext
            .DownloadTaskMovie.AsTracking()
            .Include(x => x.Children)
                .ThenInclude(x => x.DownloadWorkerTasks)
            .ToList();
        downloadTasks.ShouldNotBeNull();

        downloadTasks.SetDownloadStatus(DownloadStatus.DownloadFinished);
        await dbContext.SaveChangesAsync(CancellationToken);

        var downloadTask = container.DbContext.DownloadTaskMovieFile.First();

        // Act
        var startResult = await container.MoveDownloadFileScheduler.StartMoveDownloadFileJob(downloadTask.ToKey());
        await container.SchedulerService.AwaitScheduler(CancellationToken);

        // Assert
        startResult.IsSuccess.ShouldBeTrue();

        var downloadTaskDb = await container.DbContext.GetDownloadTaskAsync(downloadTask.ToKey(), CancellationToken);
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Completed);

        container.MockSignalRService.ServerDownloadProgressList.Count.ShouldBeGreaterThanOrEqualTo(3);
    }
}
