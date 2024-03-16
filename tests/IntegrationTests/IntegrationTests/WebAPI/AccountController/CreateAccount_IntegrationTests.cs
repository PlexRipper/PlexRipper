using Data.Contracts;
using Microsoft.EntityFrameworkCore;
using PlexRipper.WebAPI.Common;
using PlexRipper.WebAPI.Common.DTO;

namespace IntegrationTests.WebAPI.AccountController;

public class CreateAccount_IntegrationTests : BaseIntegrationTests
{
    public CreateAccount_IntegrationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldCreateAndInspectAccessibleServers_WhenPlexAccountIsValid()
    {
        // Arrange
        var libraryCount = 3;
        SpinUpPlexServer(config => { config.FakeDataConfig = dataConfig => { dataConfig.LibraryCount = libraryCount; }; });
        SetupMockPlexApi(config => config.AccessiblePlexServers = 1);
        await CreateContainer();

        var plexAccount = FakeData.GetPlexAccount(4347564).Generate();
        var plexAccountDTO = Container.Mapper.Map<PlexAccountDTO>(plexAccount);

        // Act
        var response = await Container.ApiClient.PostAsJsonAsync(ApiRoutes.Account.PostCreateAccount, plexAccountDTO);
        var resultDTO = await response.Deserialize<PlexAccountDTO>();
        resultDTO.IsSuccess.ShouldBeTrue();
        var result = Container.Mapper.Map<Result<PlexAccountDTO>>(resultDTO);
        await Container.SchedulerService.AwaitScheduler();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        DbContext.PlexAccounts.ToList().Count.ShouldBe(1);

        // Ensure account has been created
        var plexAccountDb = DbContext.PlexAccounts
            .Include(x => x.PlexAccountServers)
            .ThenInclude(x => x.PlexServer)
            .Include(x => x.PlexAccountLibraries)
            .FirstOrDefault();
        plexAccountDb.IsValidated = true;
        plexAccountDb.PlexServers.Count.ShouldBe(1);
        plexAccountDb.DisplayName.ShouldBe(plexAccountDTO.DisplayName);
        plexAccountDb.Username.ShouldBe(plexAccountDTO.Username);
        plexAccountDb.Password.ShouldBe(plexAccountDTO.Password);
        plexAccountDb.PlexAccountLibraries.Count.ShouldBe(libraryCount);

        // Ensure PlexServer has been created
        DbContext.PlexServers.ToList().Count.ShouldBe(1);
        var plexServersDb = DbContext.PlexServers.IncludeLibrariesWithMedia().FirstOrDefault();
        plexServersDb.MachineIdentifier.ShouldNotBeEmpty();
        plexServersDb.PlexLibraries.Count.ShouldBe(libraryCount);

        // Ensure All PlexLibraries have been created with media
        var plexLibraries = plexServersDb.PlexLibraries;
        plexLibraries.ShouldAllBe(x => x.HasMedia);
    }
}