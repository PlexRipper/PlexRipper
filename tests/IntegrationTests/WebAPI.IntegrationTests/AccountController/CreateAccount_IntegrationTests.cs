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
        SpinUpPlexServers(list => { list.Add(new PlexMockServerConfig()); });

        SetupMockPlexApi(config => config.AccessiblePlexServers = 1);
        await CreateContainer(config => { config.Seed = 4564; });

        var plexAccount = FakeData.GetPlexAccount(config => config.Seed = 4347564).Generate();
        var plexAccountDTO = Container.Mapper.Map<PlexAccountDTO>(plexAccount);

        // Act
        var response = await Container.ApiClient.PostAsJsonAsync(ApiRoutes.Account.PostCreateAccount, plexAccountDTO);
        var resultDTO = await response.Deserialize<PlexAccountDTO>();
        var result = Container.Mapper.Map<Result<PlexAccountDTO>>(resultDTO);
        await Container.SchedulerService.AwaitScheduler();

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }
}