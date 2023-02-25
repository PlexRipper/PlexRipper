using Data.Contracts;
using Xunit;
using Xunit.Abstractions;
using PlexRipper.BaseTests;

namespace FileSystem.IntegrationTests.FileMerger;

[Collection("Sequential")]
public class FileMergeScheduler_StartFileMergeJob_IntegrationTests : BaseIntegrationTests
{
    public FileMergeScheduler_StartFileMergeJob_IntegrationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldFinishMergingDownloadTaskAsFileTaskJobAndSetToCompleted_WhenDownloadTaskHasFinishedDownloading()
    {
        // Arrange
        Seed = 4564;
        var serverUri = SpinUpPlexServer(config => { config.DownloadFileSizeInMb = 50; });
        await SetupDatabase(config =>
        {
            config.MockServerUris.Add(serverUri);
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 3;
            config.MovieCount = 1;
            config.MovieDownloadTasksCount = 1;
            config.DownloadFileSizeInMb = 50;
        });

        SetupMockPlexApi();

        await CreateContainer(config => { config.DownloadSpeedLimitInKib = 5000; });

        var downloadTask = DbContext
            .DownloadTasks
            .FirstOrDefault(x => x.DownloadTaskType == DownloadTaskType.MovieData);
        downloadTask.ShouldNotBeNull();

        // Act
       var downloadWorkerTasks = Container.GetDownloadTaskFactory.GenerateDownloadWorkerTasks(downloadTask);
        var addWorkersResult = await Container.Mediator.Send(new AddDownloadWorkerTasksCommand(downloadWorkerTasks.Value));
        addWorkersResult.IsSuccess.ShouldBeTrue();
        var createResult =  await Container.FileMergeScheduler.CreateFileTaskFromDownloadTask(downloadTask.Id);
        createResult.IsSuccess.ShouldBeTrue();
        var startResult = await Container.FileMergeScheduler.StartFileMergeJob(createResult.Value.Id);
        await Container.SchedulerService.AwaitScheduler();

        // Assert
        startResult.IsSuccess.ShouldBeTrue();
        var downloadTaskResult = await Container.Mediator.Send(new GetDownloadTaskByIdQuery(downloadTask.RootDownloadTaskId, true));
        downloadTaskResult.IsSuccess.ShouldBeTrue();
        var downloadTaskDb = downloadTaskResult.Value;
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Completed);
        foreach (var childDownloadTask in downloadTaskDb.Children)
        {
            childDownloadTask.DownloadStatus.ShouldBe(DownloadStatus.Completed);
        }
        Container.MockSignalRService.FileMergeProgressList.Count.ShouldBeGreaterThan(10);

    }
}