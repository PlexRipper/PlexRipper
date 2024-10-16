using Data.Contracts;

namespace IntegrationTests.FileSystem.FileMerger;

public class FileMergeScheduler_StartFileMergeJob_IntegrationTests : BaseIntegrationTests
{
    public FileMergeScheduler_StartFileMergeJob_IntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldFinishMergingDownloadTaskAsFileTaskJobAndSetToCompleted_WhenDownloadTaskHasFinishedDownloading()
    {
        // Arrange
        using var Container = await CreateContainer(config =>
        {
            config.DatabaseOptions = x =>
            {
                x.PlexServerCount = 1;
                x.PlexLibraryCount = 3;
                x.MovieCount = 1;
                x.MovieDownloadTasksCount = 1;
                x.DownloadFileSizeInMb = 10;
            };
        });
        var dbContext = Container.DbContext;
        var downloadTask = dbContext.DownloadTaskMovieFile.First();
        downloadTask.ShouldNotBeNull();

        // Act
        var downloadWorkerTasks = downloadTask.GenerateDownloadWorkerTasks(4);
        dbContext.DownloadWorkerTasks.AddRange(downloadWorkerTasks);
        await dbContext.SaveChangesAsync();

        var createResult = await Container.FileMergeScheduler.CreateFileTaskFromDownloadTask(downloadTask.ToKey());
        createResult.IsSuccess.ShouldBeTrue();
        var startResult = await Container.FileMergeScheduler.StartFileMergeJob(createResult.Value.Id);
        await Container.SchedulerService.AwaitScheduler();

        // Assert
        startResult.IsSuccess.ShouldBeTrue();

        var downloadTaskDb = await Container.DbContext.GetDownloadTaskAsync(downloadTask.Id);
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Completed);

        Container.MockSignalRService.FileMergeProgressList.Count.ShouldBeGreaterThan(10);
    }
}
