using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using PlexRipper.Application;

namespace IntegrationTests.WebAPI.DownloadController;

public class DownloadController_StartCommand_IntegrationTests : BaseIntegrationTests
{
    public DownloadController_StartCommand_IntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldStartQueuedMovieDownloadTaskOnStartCommand_WhenNoTasksAreDownloading()
    {
        // Arrange
        using var Container = await CreateContainer(config =>
        {
            config.Seed = 5594564;
            config.PlexMockApiOptions = x =>
            {
                x.MockServers.Add(new PlexMockServerConfig { DownloadFileSizeInMb = 50 });
            };
            config.DatabaseOptions = x =>
            {
                x.PlexAccountCount = 1;
                x.PlexServerCount = 1;
                x.PlexLibraryCount = 2;
                x.MovieCount = 10;
                x.MovieDownloadTasksCount = 1;
            };
        });
        var downloadTasks = await Container.DbContext.GetAllDownloadTasksByServerAsync();
        downloadTasks.Count.ShouldBe(1);
        var downloadTask = downloadTasks[0].Children[0];

        // Act
        var response = await Container.ApiClient.GETAsync<
            StartDownloadTaskEndpoint,
            StartDownloadTaskEndpointRequest,
            ResultDTO
        >(new StartDownloadTaskEndpointRequest(downloadTask.Id));
        response.Response.IsSuccessStatusCode.ShouldBeTrue();

        await Container.SchedulerService.AwaitScheduler();
        await Task.Delay(2000);

        // Assert
        var result = response.Result;
        result.IsSuccess.ShouldBeTrue();
        var downloadTaskDb = await Container.DbContext.GetDownloadTaskAsync(downloadTask.Id);
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Completed);
    }
}
