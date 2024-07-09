using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using PlexRipper.Application;

namespace IntegrationTests.WebAPI.DownloadController;

public class DownloadController_RestartCommand_IntegrationTests : BaseIntegrationTests
{
    public DownloadController_RestartCommand_IntegrationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldRestartCompletedMovieDownloadTaskOnRestartCommand_WhenTaskIsDoneDownloading()
    {
        // Arrange
        Seed = 5594564;
        var serverUri = SpinUpPlexServer(config => { config.DownloadFileSizeInMb = 50; });

        await SetupDatabase(config =>
        {
            config.MockServerUris.Add(serverUri);
            config.PlexAccountCount = 1;
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 2;
            config.MovieCount = 10;
            config.MovieDownloadTasksCount = 1;
        });

        await CreateContainer();
        var downloadTasks = await DbContext.GetAllDownloadTasksByServerAsync();
        downloadTasks.Count.ShouldBe(1);
        var downloadTask = downloadTasks[0].Children[0];

        await DbContext.SetDownloadStatus(downloadTask.ToKey(), DownloadStatus.Completed);

        // Act
        var response =
            await Container.ApiClient.GETAsync<RestartDownloadTaskEndpoint, RestartDownloadTaskEndpointRequest, ResultDTO>(
                new RestartDownloadTaskEndpointRequest(downloadTask.Id));
        response.Response.IsSuccessStatusCode.ShouldBeTrue();

        var downloadTaskDb = await DbContext.GetDownloadTaskAsync(downloadTask.Id);
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Queued);

        await Container.SchedulerService.AwaitScheduler();
        await Task.Delay(2000);

        // Assert
        var result = response.Result;
        result.IsSuccess.ShouldBeTrue();
        downloadTaskDb = await DbContext.GetDownloadTaskAsync(downloadTask.Id);
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Completed);
    }
}