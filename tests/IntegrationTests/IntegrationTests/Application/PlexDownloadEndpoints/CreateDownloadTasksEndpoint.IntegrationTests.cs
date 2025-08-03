using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;

namespace IntegrationTests;

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

        var plexMovies = await container.DbContext.PlexMovies.ToListAsync();
        plexMovies.Count.ShouldBe(
            plexMovieCount,
            $"PlexMovies count should be 10 failed with database name: {container.DbContext.DatabaseName}"
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
        await Task.Delay(2000, TestContext.Current.CancellationToken);
        await container.SchedulerService.AwaitScheduler(TestContext.Current.CancellationToken);
        await Task.Delay(2000, TestContext.Current.CancellationToken);

        // Assert
        var result = testResult.Result;
        result.IsSuccess.ShouldBeTrue();
        var downloadTasksDb = await container.DbContext.GetAllDownloadTasksByServerAsync();
        downloadTasksDb.ShouldNotBeNull();
        downloadTasksDb.ShouldNotBeEmpty();
        downloadTasksDb.Count.ShouldBe(plexMovieCount);
        downloadTasksDb.ShouldAllBe(x => x.DownloadStatus == DownloadStatus.Completed);
        downloadTasksDb.SelectMany(x => x.Children).ShouldAllBe(x => x.DownloadStatus == DownloadStatus.Completed);
    }
}
