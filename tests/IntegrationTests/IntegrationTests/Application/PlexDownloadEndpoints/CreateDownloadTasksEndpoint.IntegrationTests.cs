using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.IntegrationTests;

public class CreateDownloadTasksEndpointIntegrationTests : BaseIntegrationTests
{
    public CreateDownloadTasksEndpointIntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldDownloadMultipleMovieDownloadTasks_WhenDownloadTasksAreCreated()
    {
        // Arrange
        var plexMovieCount = 3;

        var seed = new Seed(231156);
        using var container = await CreateContainer(
            seed,
            config =>
            {
                config.DownloadSpeedLimitInKib = 25000;
                config.DatabaseOptions = x =>
                {
                    x.PlexAccountCount = 1;
                    x.PlexServerCount = 1;
                    x.PlexMovieLibraryCount = 1;
                    x.MovieCount = plexMovieCount;
                };

                config.HttpClientOptions = (x, _) =>
                {
                    x.SetupIdentityRequest(seed);
                    x.SetupDownloadFile(10);
                };
            }
        );

        // Set DownloadSegments to 1 to avoid MockFileSystem concurrency issues
        // See: https://github.com/TestableIO/System.IO.Abstractions/issues/1131
        var downloadManagerSettings = container.Resolve<IDownloadManagerSettings>();
        downloadManagerSettings.DownloadSegments = 1;

        var plexMovies = await container.DbContext.PlexMovies.ToListAsync(CancellationToken);
        plexMovies.Count.ShouldBe(
            plexMovieCount,
            $"PlexMovies count should be {plexMovieCount} failed with database name: {container.DbContext.DatabaseName}"
        );

        var dtoList = new List<DownloadMediaDTO>
        {
            new()
            {
                Type = PlexMediaType.Movie,
                MediaIds = plexMovies.Select(x => x.Id).ToList(),
                PlexServerId = plexMovies.First().PlexServerId,
                PlexLibraryId = plexMovies.First().PlexLibraryId,
                Qualities = [],
            },
        };

        // Act
        var client = container.GetApiClient();
        await client.SignIn();

        var testResult = await client.POSTAsync<
            CreateDownloadTasksEndpoint,
            CreateDownloadTasksEndpointRequest,
            BaseResultDTO
        >(new CreateDownloadTasksEndpointRequest { Request = new CreateDownloadTasksRequest(dtoList) });
        testResult.Response.IsSuccessStatusCode.ShouldBeTrue(
            $"Response status code was {testResult.Response.StatusCode}"
        );

        // Wait for all downloads to complete sequentially
        // Since only one download can run per server at a time, we need to wait for each download to finish
        await WaitForDatabaseConditionAsync(
            () =>
            {
                var tasks = container
                    .DbContext.GetAllDownloadTasksByServerAsync(cancellationToken: CancellationToken)
                    .GetAwaiter()
                    .GetResult();
                return tasks.Count == plexMovieCount
                    && tasks.All(x => x.DownloadStatus == DownloadStatus.Completed)
                    && tasks.SelectMany(x => x.Children).All(x => x.DownloadStatus == DownloadStatus.Completed);
            },
            maxRetries: 60,
            delayMs: 1000
        );

        // Assert - verify download tasks were created successfully
        var result = testResult.Result;
        result.IsSuccess.ShouldBeTrue();

        var downloadTasksDb = await container.DbContext.GetAllDownloadTasksByServerAsync(
            cancellationToken: CancellationToken
        );
        downloadTasksDb.ShouldNotBeNull();
        downloadTasksDb.ShouldNotBeEmpty();
        downloadTasksDb.Count.ShouldBe(plexMovieCount);
        downloadTasksDb.ShouldAllBe(x => x.DownloadStatus == DownloadStatus.Completed);
        downloadTasksDb.SelectMany(x => x.Children).ShouldAllBe(x => x.DownloadStatus == DownloadStatus.Completed);
    }
}
