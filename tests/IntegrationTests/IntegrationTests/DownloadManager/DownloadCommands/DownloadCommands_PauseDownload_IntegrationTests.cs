using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using PlexRipper.Application;

namespace IntegrationTests.DownloadManager.DownloadCommands;

public class DownloadCommands_PauseDownload_IntegrationTests : BaseIntegrationTests
{
    public DownloadCommands_PauseDownload_IntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldPauseADownloadTask_WhenDownloadTaskIsInProgressAndIsPaused()
    {
        // Arrange

        await CreateContainer(config =>
        {
            config.DownloadSpeedLimitInKib = 5000;
            config.DatabaseOptions = x =>
            {
                x.PlexAccountCount = 1;
                x.PlexServerCount = 1;
                x.PlexLibraryCount = 1;
                x.MovieCount = 1;
                x.MovieDownloadTasksCount = 1;
                x.DownloadFileSizeInMb = 50;
            };

            config.PlexMockApiOptions = x =>
            {
                x.MockServers.Add(new PlexMockServerConfig() { DownloadFileSizeInMb = 50 });
            };
        });

        var downloadTasks = await DbContext.GetAllDownloadTasksByServerAsync();
        var childDownloadTask = downloadTasks[0].Children[0];

        // Act
        var response = await Container.ApiClient.GETAsync<
            StartDownloadTaskEndpoint,
            StartDownloadTaskEndpointRequest,
            ResultDTO
        >(new StartDownloadTaskEndpointRequest(childDownloadTask.Id));
        var startResult = response.Result;
        response.Response.IsSuccessStatusCode.ShouldBeTrue(startResult.ToString());
        await Task.Delay(500);

        response = await Container.ApiClient.GETAsync<
            PauseDownloadTaskEndpoint,
            PauseDownloadTaskEndpointRequest,
            ResultDTO
        >(new PauseDownloadTaskEndpointRequest(childDownloadTask.Id));
        var pauseResult = response.Result;
        response.Response.IsSuccessStatusCode.ShouldBeTrue(pauseResult.ToString());

        await Container.SchedulerService.AwaitScheduler();

        // Assert

        startResult.IsSuccess.ShouldBeTrue();
        pauseResult.IsSuccess.ShouldBeTrue();
        var downloadTaskDb = DbContext.DownloadTaskMovieFile.FirstOrDefault(x => x.Id == childDownloadTask.Id);
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Paused);
    }
}
