using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;

namespace IntegrationTests.AccountController;

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
                    // Start from an empty database
                    x.PlexServerCount = 0;
                    x.PlexMovieLibraryCount = 0;
                    x.PlexAccountCount = 0;
                    x.PlexServerConnectionPerServerCount = 1;
                };
                config.BaseMockHttpClientOptions = x =>
                {
                    x.PlexServerAccessCount = serverCount;
                    x.MovieLibraryCount = libraryCount;
                    x.MoviesPerLibraryCount = 500;
                };
            }
        );

        var plexAccount = FakeData.GetPlexAccount(4347564).Generate();
        var plexAccountDTO = plexAccount.ToDTO();

        // Act
        var client = container.GetApiClient();
        await client.SignIn();
        var response = await client.POSTAsync<
            CreatePlexAccountEndpoint,
            CreatePlexAccountEndpointRequest,
            ResultDTO<PlexAccountDTO>
        >(new CreatePlexAccountEndpointRequest { PlexAccount = plexAccountDTO });
        response.Response.IsSuccessStatusCode.ShouldBeTrue();

        var resultDTO = response.Result;
        resultDTO.IsSuccess.ShouldBeTrue();
        await container.SchedulerService.AwaitScheduler();

        // Assert
        resultDTO.IsSuccess.ShouldBeTrue();
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
        container.DbContext.PlexServers.ToList().Count.ShouldBe(serverCount);
        var plexServersDb = container
            .DbContext.PlexServers.Include(x => x.PlexLibraries)
            .IncludeLibrariesWithMedia()
            .FirstOrDefault();
        plexServersDb.ShouldNotBeNull();
        plexServersDb.MachineIdentifier.ShouldNotBeEmpty();
        plexServersDb.PlexLibraries.Count.ShouldBe(libraryCount);

        // Ensure all jobs have sent notifications
        // TODO: Keeps breaking due to the order of the jobs being executed
        var jobStatusUpdateList = container.MockSignalRService.JobStatusUpdateList.ToList();
        jobStatusUpdateList.Count.ShouldBe(4);

        jobStatusUpdateList[0].JobType.ShouldBe(JobTypes.InspectPlexServerJob);
        jobStatusUpdateList[0].Status.ShouldBe(JobStatus.Started);
        jobStatusUpdateList[1].JobType.ShouldBe(JobTypes.InspectPlexServerJob);
        jobStatusUpdateList[1].Status.ShouldBe(JobStatus.Completed);
        jobStatusUpdateList[2].JobType.ShouldBe(JobTypes.SyncServerMediaJob);
        jobStatusUpdateList[2].Status.ShouldBe(JobStatus.Started);
        jobStatusUpdateList[3].JobType.ShouldBe(JobTypes.SyncServerMediaJob);
        jobStatusUpdateList[3].Status.ShouldBe(JobStatus.Completed);
    }
}