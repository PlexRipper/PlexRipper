using System.Net.Http.Json;
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
        await CreateContainer(config =>
        {
            config.Seed = 4564;
            config.SetupMockPlexApi();
        });

        var plexAccount = FakeData.GetPlexAccount(config => config.Seed = 4347564).Generate();
        var plexAccountDTO = Container.Mapper.Map<PlexAccountDTO>(plexAccount);

        // Act
        var response = await Container.ApiClient.PostAsJsonAsync(ApiRoutes.Account.PostCreateAccount, plexAccountDTO);
        var resultDTO = await response.Deserialize<PlexAccountDTO>();
        var result = Container.Mapper.Map<Result<PlexAccountDTO>>(resultDTO);
        await Task.Delay(5000);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }
}