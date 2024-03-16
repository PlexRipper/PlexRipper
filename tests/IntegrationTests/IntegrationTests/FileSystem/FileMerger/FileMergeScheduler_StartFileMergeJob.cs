using Data.Contracts;

namespace IntegrationTests.FileSystem.FileMerger;

public class FileMergeScheduler_StartFileMergeJob_IntegrationTests : BaseIntegrationTests
{
    public FileMergeScheduler_StartFileMergeJob_IntegrationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldFinishMergingDownloadTaskAsFileTaskJobAndSetToCompleted_WhenDownloadTaskHasFinishedDownloading()
    {
        // Arrange

        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 3;
            config.MovieCount = 1;
            config.MovieDownloadTasksCount = 1;
            config.DownloadFileSizeInMb = 10;
        });

        await CreateContainer();
        var dbContext = DbContext;
        var downloadTask = dbContext.DownloadTaskMovieFile.First();
        downloadTask.ShouldNotBeNull();

        // Act
        var downloadWorkerTasks = downloadTask.ToGeneric().GenerateDownloadWorkerTasks(4);
        dbContext.DownloadWorkerTasks.AddRange(downloadWorkerTasks);
        await dbContext.SaveChangesAsync();

        var createResult = await Container.FileMergeScheduler.CreateFileTaskFromDownloadTask(downloadTask.ToKey());
        createResult.IsSuccess.ShouldBeTrue();
        var startResult = await Container.FileMergeScheduler.StartFileMergeJob(createResult.Value.Id);
        await Container.SchedulerService.AwaitScheduler();

        // Assert
        startResult.IsSuccess.ShouldBeTrue();

        var downloadTaskDb = await DbContext.GetDownloadTaskAsync(downloadTask.Id);
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Completed);

        Container.MockSignalRService.FileMergeProgressList.Count.ShouldBeGreaterThan(10);
    }
}