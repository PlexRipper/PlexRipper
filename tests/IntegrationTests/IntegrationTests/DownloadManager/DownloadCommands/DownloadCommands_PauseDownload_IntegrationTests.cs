using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using PlexRipper.Application;
using Serilog.Events;

namespace IntegrationTests.DownloadManager.DownloadCommands;

public class DownloadCommands_PauseDownload_IntegrationTests : BaseIntegrationTests
{
    public DownloadCommands_PauseDownload_IntegrationTests(ITestOutputHelper output) : base(output, LogEventLevel.Verbose) { }

    [Fact]
    public async Task ShouldPauseADownloadTask_WhenDownloadTaskIsInProgressAndIsPaused()
    {
        // Arrange

        var serverUri = SpinUpPlexServer(config => { config.DownloadFileSizeInMb = 50; });
        await SetupDatabase(config =>
        {
            config.MockServerUris.Add(serverUri);
            config.PlexAccountCount = 1;
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.MovieCount = 1;
            config.MovieDownloadTasksCount = 1;
            config.DownloadFileSizeInMb = 50;
        });

        SetupMockPlexApi();

        await CreateContainer(config => config.DownloadSpeedLimitInKib = 5000);

        var downloadTasks = await DbContext.GetAllDownloadTasksAsync();
        var childDownloadTask = downloadTasks[0].Children[0];

        // Act
        var response =
            await Container.ApiClient.GETAsync<StartDownloadTaskEndpoint, StartDownloadTaskEndpointRequest, ResultDTO>(
                new StartDownloadTaskEndpointRequest(childDownloadTask.Id));
        var startResult = response.Result;
        response.Response.IsSuccessStatusCode.ShouldBeTrue(startResult.ToString());
        await Task.Delay(2000);

        response = await Container.ApiClient.GETAsync<PauseDownloadTaskEndpoint, PauseDownloadTaskEndpointRequest, ResultDTO>(
            new PauseDownloadTaskEndpointRequest(childDownloadTask.Id));
        var pauseResult = response.Result;
        response.Response.IsSuccessStatusCode.ShouldBeTrue(pauseResult.ToString());

        await Container.SchedulerService.AwaitScheduler();
        await Task.Delay(2000);

        // Assert

        startResult.IsSuccess.ShouldBeTrue();
        pauseResult.IsSuccess.ShouldBeTrue();
        var downloadTaskDb = DbContext.DownloadTaskMovieFile.FirstOrDefault(x => x.Id == childDownloadTask.Id);
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Paused);
    }
}