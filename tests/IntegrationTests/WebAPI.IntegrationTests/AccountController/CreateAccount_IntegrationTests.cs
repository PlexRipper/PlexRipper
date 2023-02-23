using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;
using PlexRipper.WebAPI.Common;
using PlexRipper.WebAPI.Common.DTO;

namespace WebAPI.IntegrationTests.AccountController;

[Collection("Sequential")]
public class CreateAccount_IntegrationTests : BaseIntegrationTests
{
    public CreateAccount_IntegrationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldCreateAndInspectAccessibleServers_WhenPlexAccountIsValid()
    {
        // Arrange
        Seed = 4564;
        var libraryCount = 3;
        SpinUpPlexServer(config => { config.FakeDataConfig = dataConfig => { dataConfig.LibraryCount = libraryCount; }; });
        SetupMockPlexApi(config => config.AccessiblePlexServers = 1);
        await CreateContainer();

        var plexAccount = FakeData.GetPlexAccount(4347564).Generate();
        var plexAccountDTO = Container.Mapper.Map<PlexAccountDTO>(plexAccount);
       // var plexAccountDTOJson = JsonSerializer.Serialize(plexAccountDTO);



       // Act
        var response = await Container.ApiClient.PostAsJsonAsync(ApiRoutes.Account.PostCreateAccount, plexAccountDTO);
        var resultDTO = await response.Deserialize<PlexAccountDTO>();
        resultDTO.IsSuccess.ShouldBeTrue();
        var result = Container.Mapper.Map<Result<PlexAccountDTO>>(resultDTO);
        await Container.SchedulerService.AwaitScheduler();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var db = Container.PlexRipperDbContext;
        db.PlexAccounts.ToList().Count.ShouldBe(1);

        // Ensure account has been created
        var plexAccountDb = db.PlexAccounts
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
        db.PlexServers.ToList().Count.ShouldBe(1);
        var plexServersDb = db.PlexServers.IncludeLibrariesWithMedia().FirstOrDefault();
        plexServersDb.MachineIdentifier.ShouldNotBeEmpty();
        plexServersDb.PlexLibraries.Count.ShouldBe(libraryCount);

        // Ensure All PlexLibraries have been created with media
        var plexLibraries = plexServersDb.PlexLibraries;
        plexLibraries.ShouldAllBe(x => x.HasMedia);
    }
}