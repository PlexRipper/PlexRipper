using System.Net;
using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Moq.Contrib.HttpClient;
using PlexRipper.Application;

namespace IntegrationTests.WebAPI.AccountController;

public class CreateAccountIntegrationTests : BaseIntegrationTests
{
    public CreateAccountIntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldCreateAndInspectAccessibleServers_WhenPlexAccountIsValid()
    {
        // Arrange
        var seed = new Seed(236234);
        var serverCount = 1;
        var libraryCount = 3;

        using var container = await CreateContainer(
            seed.Next(),
            config =>
            {
                config.DatabaseOptions = x =>
                {
                    x.PlexLibraryCount = libraryCount;
                };
                config.HttpClientOptions = x =>
                {
                    var response1 = FakePlexApiData.GetServerResourcesResponse(
                        HttpStatusCode.OK,
                        new Seed(939),
                        null,
                        y => y.PlexServerAccessCount = serverCount
                    );

                    x.SetupRequest(HttpMethod.Get, "https://plex.tv/api/v2/resources")
                        .ReturnsAsync(response1.RawResponse);

                    x.SetupRequest(
                            HttpMethod.Get,
                            "https://plex.tv/api/v2/resources?includeHttps=1&includeRelay=1&includeIPv6=1"
                        )
                        .ReturnsAsync(
                            (HttpRequestMessage req, CancellationToken _) =>
                                FakePlexApiData
                                    .GetServerResourcesResponse(
                                        HttpStatusCode.OK,
                                        new Seed(940),
                                        req,
                                        y =>
                                        {
                                            y.PlexServerAccessCount = serverCount;
                                            y.PlexServerAccessConnectionsIncludeHttps = true;
                                        }
                                    )
                                    .RawResponse
                        );

                    response1.PlexDevices.ShouldNotBeNull();

                    foreach (var connection in response1.PlexDevices.SelectMany(device => device.Connections))
                    {
                        x.SetupIdentityRequest(seed, connection.Uri);

                        x.SetupRequest(connection.Uri + "library/sections")
                            .ReturnsAsync(
                                (HttpRequestMessage req, CancellationToken _) =>
                                    FakePlexApiData.GetAllLibrariesResponse(HttpStatusCode.OK, seed, req).RawResponse
                            );
                    }
                };
            }
        );

        var plexAccount = FakeData.GetPlexAccount(4347564).Generate();
        var plexAccountDTO = plexAccount.ToDTO();

        // Act
        var response = await container.ApiClient.POSTAsync<
            CreatePlexAccountEndpoint,
            PlexAccountDTO,
            ResultDTO<PlexAccount>
        >(plexAccountDTO);
        response.Response.IsSuccessStatusCode.ShouldBeTrue();

        var resultDTO = response.Result;
        resultDTO.IsSuccess.ShouldBeTrue();
        var result = resultDTO.ToResultModel();
        await container.SchedulerService.AwaitScheduler();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        container.DbContext.PlexAccounts.ToList().Count.ShouldBe(1);

        // Ensure account has been created
        var plexAccountDb = container
            .DbContext.PlexAccounts.Include(x => x.PlexAccountLibraries)
            .ThenInclude(x => x.PlexLibrary)
            .Include(x => x.PlexAccountServers)
            .ThenInclude(x => x.PlexServer)
            .FirstOrDefault();

        plexAccountDb.ShouldNotBeNull();
        plexAccountDb.IsValidated = true;
        plexAccountDb.PlexServers.Count.ShouldBe(1);
        plexAccountDb.DisplayName.ShouldBe(plexAccountDTO.DisplayName);
        plexAccountDb.Username.ShouldBe(plexAccountDTO.Username);
        plexAccountDb.Password.ShouldBe(plexAccountDTO.Password);
        plexAccountDb.PlexAccountLibraries.Count.ShouldBe(libraryCount);

        // Ensure PlexServer has been created
        container.DbContext.PlexServers.ToList().Count.ShouldBe(2);
        var plexServersDb = container
            .DbContext.PlexServers.Include(x => x.PlexLibraries)
            .IncludeLibrariesWithMedia()
            .FirstOrDefault();
        plexServersDb.ShouldNotBeNull();
        plexServersDb.MachineIdentifier.ShouldNotBeEmpty();
        plexServersDb.PlexLibraries.Count.ShouldBe(libraryCount);

        // Ensure all jobs have sent notifications
        // TODO: Keeps breaking due to the order of the jobs being executed
        // var jobStatusUpdateList = Container.MockSignalRService.JobStatusUpdateList.ToList();
        // jobStatusUpdateList.Count.ShouldBe(8);

        // jobStatusUpdateList[0].JobType.ShouldBe(JobTypes.InspectPlexServerJob);
        // jobStatusUpdateList[0].Status.ShouldBe(JobStatus.Started);
        // jobStatusUpdateList[1].JobType.ShouldBe(JobTypes.CheckAllConnectionsStatusByPlexServerJob);
        // jobStatusUpdateList[1].Status.ShouldBe(JobStatus.Started);
        // jobStatusUpdateList[2].JobType.ShouldBe(JobTypes.CheckAllConnectionsStatusByPlexServerJob);
        // jobStatusUpdateList[2].Status.ShouldBe(JobStatus.Completed);
        // jobStatusUpdateList[3].JobType.ShouldBe(JobTypes.InspectPlexServerJob);
        // jobStatusUpdateList[3].Status.ShouldBe(JobStatus.Completed);
        // jobStatusUpdateList[4].JobType.ShouldBe(JobTypes.SyncServerMediaJob);
        // jobStatusUpdateList[4].Status.ShouldBe(JobStatus.Started);
        // jobStatusUpdateList[5].JobType.ShouldBe(JobTypes.SyncServerMediaJob);
        // jobStatusUpdateList[5].Status.ShouldBe(JobStatus.Completed);
    }
}
