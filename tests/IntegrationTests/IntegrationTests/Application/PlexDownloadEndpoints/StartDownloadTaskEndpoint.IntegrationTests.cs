using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.IntegrationTests;

public class StartDownloadTaskEndpointIntegrationTests : BaseIntegrationTests
{
    public StartDownloadTaskEndpointIntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldStartQueuedMovieDownloadTaskOnStartCommand_WhenNoTasksAreDownloading()
    {
        // Arrange
        var seed = new Seed(8932);
        using var container = await CreateContainer(
            seed,
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
                    x.DownloadWorkerTasks = 4;
                };

                config.FileSystemOptions = (system, dbContext) =>
                {
                    var downloadTask = dbContext.DownloadTaskMovieFile.Include(x => x.DownloadWorkerTasks).First();
                    downloadTask.DownloadFilePath.ShouldNotBeNullOrEmpty();

                    system.AddFile(downloadTask.DownloadFilePath, FakeData.GetFileMockData(10, 4));
                };
            }
        );
        var downloadTasks = await container.DbContext.GetAllDownloadTasksByServerAsync(
            cancellationToken: CancellationToken
        );
        downloadTasks.Count.ShouldBe(1);
        var downloadTask = downloadTasks.First().Children.FirstOrDefault();
        downloadTask.ShouldNotBeNull();

        // Act
        var client = container.GetApiClient();
        await client.SignIn();

        var testResult = await client.GETAsync<
            StartDownloadTaskEndpoint,
            StartDownloadTaskEndpointRequest,
            BaseResultDTO
        >(new StartDownloadTaskEndpointRequest(downloadTask.Id));
        testResult.Response.IsSuccessStatusCode.ShouldBeTrue();

        await container.SchedulerService.AwaitScheduler(CancellationToken);
        await Task.Delay(2000, TestContext.Current.CancellationToken);

        // Assert
        var result = testResult.Result;
        result.IsSuccess.ShouldBeTrue();
        var downloadTaskDb = await container.DbContext.GetDownloadTaskAsync(
            downloadTask.Id,
            cancellationToken: CancellationToken
        );
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Completed);
    }
}
