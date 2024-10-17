using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using PlexRipper.Application;

namespace IntegrationTests.WebAPI.DownloadController;

public class DownloadControllerRestartCommandIntegrationTests : BaseIntegrationTests
{
    public DownloadControllerRestartCommandIntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldRestartCompletedMovieDownloadTaskOnRestartCommand_WhenTaskIsDoneDownloading()
    {
        // Arrange
        var seed = new Seed(5594564);
        using var container = await CreateContainer(
            5594564,
            config =>
            {
                config.HttpClientOptions = x =>
                {
                    x.SetupIdentityRequest(seed);
                    x.SetupDownloadFile(10);
                };

                config.DatabaseOptions = x =>
                {
                    x.PlexAccountCount = 1;
                    x.PlexServerCount = 1;
                    x.PlexLibraryCount = 2;
                    x.MovieCount = 10;
                    x.MovieDownloadTasksCount = 1;
                };
            }
        );
        var downloadTasks = await container.DbContext.GetAllDownloadTasksByServerAsync();
        downloadTasks.Count.ShouldBe(1);
        var downloadTask = downloadTasks[0].Children[0];

        await container.DbContext.SetDownloadStatus(downloadTask.ToKey(), DownloadStatus.Completed);

        // Act
        var response = await container.ApiClient.GETAsync<
            RestartDownloadTaskEndpoint,
            RestartDownloadTaskEndpointRequest,
            ResultDTO
        >(new RestartDownloadTaskEndpointRequest(downloadTask.Id));
        response.Response.IsSuccessStatusCode.ShouldBeTrue(await response.Response.Content.ReadAsStringAsync());

        var downloadTaskDb = await container.DbContext.GetDownloadTaskAsync(downloadTask.Id);
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Queued);

        await container.SchedulerService.AwaitScheduler();
        await Task.Delay(2000);

        // Assert
        var result = response.Result;
        result.IsSuccess.ShouldBeTrue();
        downloadTaskDb = await container.DbContext.GetDownloadTaskAsync(downloadTask.Id);
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Completed);
    }
}
