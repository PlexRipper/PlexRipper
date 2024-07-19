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
        SpinUpPlexServer(config =>
        {
            config.FakeDataConfig = dataConfig =>
            {
                dataConfig.LibraryCount = libraryCount;
            };
        });
        SetupMockPlexApi(config => config.AccessiblePlexServers = 1);
        await CreateContainer();

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

        var jobStatusList = Container.MockSignalRService.JobStatusUpdateList;
        jobStatusList.ShouldContain(x => x.JobType == JobTypes.RefreshPlexServersAccessJob);

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
        DbContext.PlexServers.ToList().Count.ShouldBe(1);
        var plexServersDb = DbContext
            .PlexServers.Include(x => x.PlexLibraries)
            .IncludeLibrariesWithMedia()
            .FirstOrDefault();
        plexServersDb.ShouldNotBeNull();
        plexServersDb.MachineIdentifier.ShouldNotBeEmpty();
        plexServersDb.PlexLibraries.Count.ShouldBe(libraryCount);

        // Ensure All PlexLibraries have been created with media
        var plexLibraries = plexServersDb.PlexLibraries;
        plexLibraries.ShouldAllBe(x => x.HasMedia);

        // Ensure all jobs have sent notifications
        var jobStatusUpdateList = Container.MockSignalRService.JobStatusUpdateList.ToList();

        jobStatusUpdateList[0].JobType.ShouldBe(JobTypes.RefreshPlexServersAccessJob);
        jobStatusUpdateList[0].Status.ShouldBe(JobStatus.Started);
        jobStatusUpdateList[1].JobType.ShouldBe(JobTypes.RefreshPlexServersAccessJob);
        jobStatusUpdateList[1].Status.ShouldBe(JobStatus.Completed);
        jobStatusUpdateList[2].JobType.ShouldBe(JobTypes.CheckPlexServerConnectionsJob);
        jobStatusUpdateList[2].Status.ShouldBe(JobStatus.Started);
        jobStatusUpdateList[3].JobType.ShouldBe(JobTypes.CheckPlexServerConnectionsJob);
        jobStatusUpdateList[3].Status.ShouldBe(JobStatus.Completed);
        jobStatusUpdateList[4].JobType.ShouldBe(JobTypes.SyncServerJob);
        jobStatusUpdateList[4].Status.ShouldBe(JobStatus.Started);
        jobStatusUpdateList[5].JobType.ShouldBe(JobTypes.SyncServerJob);
        jobStatusUpdateList[5].Status.ShouldBe(JobStatus.Completed);
    }
}
