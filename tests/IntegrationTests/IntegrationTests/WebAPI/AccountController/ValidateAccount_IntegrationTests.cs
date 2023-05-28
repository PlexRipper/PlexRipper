using PlexRipper.WebAPI.Common;
using PlexRipper.WebAPI.Common.DTO;

namespace IntegrationTests.WebAPI.AccountController;


public class ValidateAccount_IntegrationTests : BaseIntegrationTests
{
    public ValidateAccount_IntegrationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldValidatePlexAccount_WhenGivenValidCredentials()
    {
        // Arrange
        SetupMockPlexApi(apiConfig => { apiConfig.SignInResponseIsValid = true; });

        await CreateContainer();

        var plexAccount = FakeData.GetPlexAccount(26346).Generate();
        var plexAccountDTO = Container.Mapper.Map<PlexAccountDTO>(plexAccount);

        // Act
        var response = await Container.ApiClient.PostAsJsonAsync(ApiRoutes.Account.PostValidate, plexAccountDTO);
        var result = await response.Deserialize<PlexAccountDTO>();

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldInValidatePlexAccountWithErrors_WhenGivenInValidCredentials()
    {
        // Arrange
        Seed = 4347564;
        SetupMockPlexApi(apiConfig => { apiConfig.SignInResponseIsValid = false; });
        await CreateContainer();

        var plexAccount = FakeData.GetPlexAccount(4347564).Generate();
        var plexAccountDTO = Container.Mapper.Map<PlexAccountDTO>(plexAccount);

        // Act
        var response = await Container.ApiClient.PostAsJsonAsync(ApiRoutes.Account.PostValidate, plexAccountDTO);
        var resultDTO = await response.Deserialize<PlexAccountDTO>();
        var result = Container.Mapper.Map<Result>(resultDTO);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.Count.ShouldBe(2);
        result.Has401UnauthorizedError().ShouldBeTrue();
    }
}