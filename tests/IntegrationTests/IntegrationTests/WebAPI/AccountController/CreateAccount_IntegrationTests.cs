using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;

namespace IntegrationTests.WebAPI.AccountController;

public class CreateAccount_IntegrationTests : BaseIntegrationTests
{
    public CreateAccount_IntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldCreateAndInspectAccessibleServers_WhenPlexAccountIsValid()
    {
        // Arrange
        var libraryCount = 3;
        await CreateContainer(config =>
        {
            config.DatabaseOptions = x =>
            {
                x.PlexLibraryCount = libraryCount;
            };
            config.PlexMockApiOptions = x =>
            {
                x.MockServers.Add(
                    new PlexMockServerConfig
                    {
                        FakeDataConfig = apiConfig =>
                        {
                            apiConfig.LibraryCount = libraryCount;
                        },
                    }
                );
            };
        });

        var plexAccount = FakeData.GetPlexAccount(4347564).Generate();
        var plexAccountDTO = plexAccount.ToDTO();

        // Act
        var response = await Container.ApiClient.POSTAsync<
            CreatePlexAccountEndpoint,
            PlexAccountDTO,
            ResultDTO<PlexAccount>
        >(plexAccountDTO);
        response.Response.IsSuccessStatusCode.ShouldBeTrue();

        var resultDTO = response.Result;
        resultDTO.IsSuccess.ShouldBeTrue();
        var result = resultDTO.ToResultModel();
        await Container.SchedulerService.AwaitScheduler();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        DbContext.PlexAccounts.ToList().Count.ShouldBe(1);

        // Ensure account has been created
        var plexAccountDb = DbContext
            .PlexAccounts.Include(x => x.PlexAccountLibraries)
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
        DbContext.PlexServers.ToList().Count.ShouldBe(2);
        var plexServersDb = DbContext
            .PlexServers.Include(x => x.PlexLibraries)
            .IncludeLibrariesWithMedia()
            .FirstOrDefault();
        plexServersDb.ShouldNotBeNull();
        plexServersDb.MachineIdentifier.ShouldNotBeEmpty();
        plexServersDb.PlexLibraries.Count.ShouldBe(libraryCount);

        // Ensure all jobs have sent notifications
        var jobStatusUpdateList = Container.MockSignalRService.JobStatusUpdateList.ToList();
        jobStatusUpdateList.Count.ShouldBe(8);

        // TODO: Keeps breaking due to the order of the jobs being executed
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
