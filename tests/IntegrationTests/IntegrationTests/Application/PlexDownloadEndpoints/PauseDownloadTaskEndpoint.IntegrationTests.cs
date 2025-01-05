using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using PlexRipper.Application;

namespace IntegrationTests;

[CollectionDefinition("Non-Parallel Tests", DisableParallelization = true)]
public class PauseDownloadTaskEndpointIntegrationTests : BaseIntegrationTests
{
    public PauseDownloadTaskEndpointIntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldPauseADownloadTask_WhenDownloadTaskIsInProgressAndIsPaused()
    {
        // Arrange
        var seed = new Seed(21345);
        using var container = await CreateContainer(
            seed,
            config =>
            {
                config.DownloadSpeedLimitInKib = 5000;
                config.DatabaseOptions = x =>
                {
                    x.PlexAccountCount = 1;
                    x.PlexServerCount = 1;
                    x.PlexLibraryCount = 1;
                    x.MovieCount = 1;
                    x.MovieDownloadTasksCount = 1;
                    x.DownloadWorkerTasks = 4;
                    x.DownloadFileSizeInMb = 50;
                };

                config.HttpClientOptions = x =>
                {
                    x.SetupIdentityRequest(seed);
                    x.SetupDownloadFile(50);
                };
            }
        );

        var downloadTasks = await container.DbContext.GetAllDownloadTasksByServerAsync();
        var childDownloadTask = downloadTasks[0].Children[0];

        // Act
        var client = container.GetApiClient();
        await client.SignIn();

        var testResult = await client.GETAsync<StartDownloadTaskEndpoint, StartDownloadTaskEndpointRequest, ResultDTO>(
            new StartDownloadTaskEndpointRequest(childDownloadTask.Id)
        );
        var startResult = testResult.Result;
        testResult.Response.IsSuccessStatusCode.ShouldBeTrue(startResult.ToString());

        testResult = await client.GETAsync<PauseDownloadTaskEndpoint, PauseDownloadTaskEndpointRequest, ResultDTO>(
            new PauseDownloadTaskEndpointRequest(childDownloadTask.Id)
        );
        var pauseResult = testResult.Result;
        testResult.Response.IsSuccessStatusCode.ShouldBeTrue(pauseResult.ToString());

        await container.SchedulerService.AwaitScheduler();

        // Assert
        startResult.IsSuccess.ShouldBeTrue();
        pauseResult.IsSuccess.ShouldBeTrue();
        var downloadTaskDb = await container.DbContext.DownloadTaskMovieFile.GetAsync(childDownloadTask.Id);
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Paused);
    }
}
