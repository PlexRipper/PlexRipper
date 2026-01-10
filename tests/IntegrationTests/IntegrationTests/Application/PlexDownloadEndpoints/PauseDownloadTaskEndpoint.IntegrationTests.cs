using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.IntegrationTests;

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
                config.DownloadSpeedLimitInKib = 500;
                config.DatabaseOptions = x =>
                {
                    x.PlexAccountCount = 1;
                    x.PlexServerCount = 1;
                    x.PlexMovieLibraryCount = 1;
                    x.MovieCount = 1;
                    x.MovieDownloadTasksCount = 1;
                    x.DownloadWorkerTasks = 4;
                    x.DownloadFileSizeInMb = 50;
                };

                config.HttpClientOptions = (x, _) =>
                {
                    x.SetupIdentityRequest(seed);
                    x.SetupDownloadFile(50);
                };

                config.FileSystemOptions = (system, dbContext) =>
                {
                    var downloadTask = dbContext.DownloadTaskMovieFile.Include(x => x.DownloadWorkerTasks).First();
                    downloadTask.DownloadFilePath.ShouldNotBeNullOrEmpty();

                    system.AddFile(downloadTask.DownloadFilePath, FakeData.GetFileMockData(50, 4));
                };
            }
        );

        var downloadTasks = await container.DbContext.GetAllDownloadTasksByServerAsync(
            cancellationToken: CancellationToken
        );
        downloadTasks.Count.ShouldBe(1);
        var childDownloadTask = downloadTasks[0].Children[0];

        // Act
        var client = container.GetApiClient();
        await client.SignIn();

        var startTestResult = await client.GETAsync<
            StartDownloadTaskEndpoint,
            StartDownloadTaskEndpointRequest,
            BaseResultDTO
        >(new StartDownloadTaskEndpointRequest(childDownloadTask.Id));
        startTestResult.Response.IsSuccessStatusCode.ShouldBeTrue(
            await startTestResult.Response.Content.ReadAsStringAsync(CancellationToken)
        );
        startTestResult.Result.IsSuccess.ShouldBeTrue();

        // Wait for the download to actually start
        await WaitForDatabaseConditionAsync(
            () =>
            {
                var task = container
                    .DbContext.DownloadTaskMovieFile.AsNoTracking()
                    .FirstOrDefault(x => x.Id == childDownloadTask.Id);
                return task?.DownloadStatus == DownloadStatus.Downloading;
            },
            maxRetries: 20,
            delayMs: 500
        );

        // Pause the download
        var pauseTestResult = await client.GETAsync<
            PauseDownloadTaskEndpoint,
            PauseDownloadTaskEndpointRequest,
            BaseResultDTO
        >(new PauseDownloadTaskEndpointRequest(childDownloadTask.Id));
        pauseTestResult.Response.IsSuccessStatusCode.ShouldBeTrue(
            await pauseTestResult.Response.Content.ReadAsStringAsync(CancellationToken)
        );
        pauseTestResult.Result.IsSuccess.ShouldBeTrue();

        // Wait for the scheduler to complete the pause operation
        await container.SchedulerService.AwaitScheduler(CancellationToken);

        // Wait for the database to reflect the paused status
        await WaitForDatabaseConditionAsync(
            () =>
            {
                var task = container
                    .DbContext.DownloadTaskMovieFile.AsNoTracking()
                    .FirstOrDefault(x => x.Id == childDownloadTask.Id);
                return task?.DownloadStatus == DownloadStatus.Paused;
            },
            maxRetries: 30,
            delayMs: 500
        );

        // Assert - use AsNoTracking to ensure fresh data from database
        var downloadTaskDb = await container
            .DbContext.DownloadTaskMovieFile.AsNoTracking()
            .Include(x => x.DownloadWorkerTasks)
            .FirstOrDefaultAsync(x => x.Id == childDownloadTask.Id, CancellationToken);
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Paused);
    }
}
