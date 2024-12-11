using Data.Contracts;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.FileSystem.FileMerger;

public class FileMergeSchedulerStartFileMergeJobIntegrationTests : BaseIntegrationTests
{
    public FileMergeSchedulerStartFileMergeJobIntegrationTests(ITestOutputHelper output)
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
                    x.PlexLibraryCount = 3;
                    x.MovieCount = 1;
                    x.MovieDownloadTasksCount = 1;
                    x.DownloadWorkerTasks = 4;
                    x.DownloadFileSizeInMb = 10;
                };

                config.FileSystemOptions = (system, dbContext) =>
                {
                    var downloadTask = dbContext.DownloadTaskMovieFile.Include(x => x.DownloadWorkerTasks).First();
                    downloadTask.FilePaths.Count.ShouldBeGreaterThan(0);
                    foreach (var filePath in downloadTask.FilePaths)
                    {
                        system.AddFile(filePath, FakeData.GetFileMockData(10, 4));
                    }
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
        await dbContext.SaveChangesAsync();

        var downloadTask = container.DbContext.DownloadTaskMovieFile.First();

        // Act
        var startResult = await container.FileMergeScheduler.StartFileMergeJob(downloadTask.ToKey());
        await container.SchedulerService.AwaitScheduler();

        // Assert
        startResult.IsSuccess.ShouldBeTrue();

        var downloadTaskDb = await container.DbContext.GetDownloadTaskAsync(downloadTask.Id);
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Completed);

        container.MockSignalRService.ServerDownloadProgressList.Count.ShouldBeGreaterThanOrEqualTo(3);
    }
}
