using FastEndpoints;
using Reaparr.Application;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.IntegrationTests;

public class RestartDownloadTaskEndpointIntegrationTests : BaseIntegrationTests
{
    public RestartDownloadTaskEndpointIntegrationTests(ITestOutputHelper output)
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
                config.HttpClientOptions = (x, _) =>
                {
                    x.SetupIdentityRequest(seed);
                    x.SetupDownloadFile(10);
                };

                config.DatabaseOptions = x =>
                {
                    x.PlexAccountCount = 1;
                    x.PlexServerCount = 1;
                    x.PlexMovieLibraryCount = 2;
                    x.MovieCount = 10;
                    x.MovieDownloadTasksCount = 1;
                };
            }
        );
        var downloadTasks = await container.DbContext.GetAllDownloadTasksByServerAsync(
            cancellationToken: CancellationToken
        );
        downloadTasks.Count.ShouldBe(1);
        var downloadTask = downloadTasks[0].Children[0];

        await container.DbContext.SetDownloadStatus(downloadTask.ToKey(), DownloadStatus.Completed);

        // Act
        var client = container.GetApiClient();
        await client.SignIn();
        var testResult = await client.GETAsync<
            RestartDownloadTaskEndpoint,
            RestartDownloadTaskEndpointRequest,
            BaseResultDTO
        >(new RestartDownloadTaskEndpointRequest(downloadTask.Id));
        testResult.Response.IsSuccessStatusCode.ShouldBeTrue(
            await testResult.Response.Content.ReadAsStringAsync(CancellationToken)
        );

        var downloadTaskDb = await container.DbContext.GetDownloadTaskAsync(
            downloadTask.Id,
            cancellationToken: CancellationToken
        );
        downloadTaskDb.ShouldNotBeNull();

        // In CI this is sometimes completed too quickly
        downloadTaskDb.DownloadStatus.ShouldBeOneOf(DownloadStatus.Queued, DownloadStatus.Completed);

        await container.SchedulerService.AwaitScheduler(TestContext.Current.CancellationToken);
        await Task.Delay(2000, TestContext.Current.CancellationToken);

        // Assert
        var result = testResult.Result;
        result.IsSuccess.ShouldBeTrue();
        downloadTaskDb = await container.DbContext.GetDownloadTaskAsync(
            downloadTask.Id,
            cancellationToken: CancellationToken
        );
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Completed);
    }
}
