using Data.Contracts;
using Microsoft.EntityFrameworkCore;

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

        var downloadTask = DbContext.DownloadTaskMovieFile.Include(x => x.DownloadWorkerTasks).First();
        downloadTask.ShouldNotBeNull();

        // Act
        downloadTask.ToGeneric().GenerateDownloadWorkerTasks(4);
        var downloadWorkerTasks = downloadTask.DownloadWorkerTasks;

        downloadWorkerTasks.ForEach(x => x.DownloadTask = null);
        await DbContext.DownloadWorkerTasks.AddRangeAsync(downloadWorkerTasks);
        await DbContext.SaveChangesAsync();

        var createResult = await Container.FileMergeScheduler.CreateFileTaskFromDownloadTask(downloadTask.ToKey());
        createResult.IsSuccess.ShouldBeTrue();
        var startResult = await Container.FileMergeScheduler.StartFileMergeJob(createResult.Value.Id);
        await Container.SchedulerService.AwaitScheduler();

        // Assert
        startResult.IsSuccess.ShouldBeTrue();

        var downloadTaskDb = await DbContext.GetDownloadTaskAsync(downloadTask.Id);
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Completed);

        foreach (var childDownloadTask in downloadTaskDb.Children)
            childDownloadTask.DownloadStatus.ShouldBe(DownloadStatus.Completed);
        Container.MockSignalRService.FileMergeProgressList.Count.ShouldBeGreaterThan(10);
    }
}