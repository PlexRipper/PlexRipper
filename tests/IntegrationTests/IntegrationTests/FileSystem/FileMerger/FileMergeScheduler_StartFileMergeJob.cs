using Data.Contracts;

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
                    x.DownloadFileSizeInMb = 10;
                };
            }
        );
        var dbContext = container.DbContext;
        var downloadTask = dbContext.DownloadTaskMovieFile.First();
        downloadTask.ShouldNotBeNull();

        // Act
        var downloadWorkerTasks = downloadTask.GenerateDownloadWorkerTasks(4);
        dbContext.DownloadWorkerTasks.AddRange(downloadWorkerTasks);
        await dbContext.SaveChangesAsync();

        var createResult = await container.FileMergeScheduler.CreateFileTaskFromDownloadTask(downloadTask.ToKey());
        createResult.IsSuccess.ShouldBeTrue();
        var startResult = await container.FileMergeScheduler.StartFileMergeJob(createResult.Value.Id);
        await container.SchedulerService.AwaitScheduler();

        // Assert
        startResult.IsSuccess.ShouldBeTrue();

        var downloadTaskDb = await container.DbContext.GetDownloadTaskAsync(downloadTask.Id);
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Completed);

        container.MockSignalRService.FileMergeProgressList.Count.ShouldBeGreaterThan(10);
    }
}
