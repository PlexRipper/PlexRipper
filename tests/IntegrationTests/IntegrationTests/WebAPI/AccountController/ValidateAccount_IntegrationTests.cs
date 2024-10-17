using System.Net;
using System.Net.Http.Json;
using Application.Contracts;
using FastEndpoints;
using Moq.Contrib.HttpClient;
using PlexRipper.Application;

namespace IntegrationTests.WebAPI.AccountController;

public class ValidateAccountIntegrationTests : BaseIntegrationTests
{
    public ValidateAccountIntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldValidatePlexAccount_WhenGivenValidCredentials()
    {
        // Arrange
        var seed = new Seed(22453);
        using var container = await CreateContainer(
            seed,
            config =>
            {
                config.HttpClientOptions = x =>
                {
                    x.SetupRequest(HttpMethod.Post, "https://plex.tv/api/v2/users/signin")
                        .ReturnsAsync(
                            (HttpRequestMessage req, CancellationToken _) =>
                                FakePlexApiData
                                    .PostUsersSignInDataResponse(HttpStatusCode.Created, seed, req)
                                    .RawResponse
                        );
                };
            }
        );

        var plexAccount = FakeData.GetPlexAccount(26346).Generate();
        var plexAccountDTO = plexAccount.ToDTO();

        // Act
        var response = await container.ApiClient.POSTAsync<
            ValidatePlexAccountEndpoint,
            PlexAccountDTO,
            ResultDTO<PlexAccountDTO>
        >(plexAccountDTO);
        response.Response.IsSuccessStatusCode.ShouldBeTrue();
        var result = response.Result;

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldInValidatePlexAccountWithErrors_WhenGivenInValidCredentials()
    {
        // Arrange
        var seed = new Seed(4347564);
        using var container = await CreateContainer(
            seed,
            config =>
                config.HttpClientOptions = x =>
                {
                    x.SetupRequest(HttpMethod.Post, "https://plex.tv/api/v2/users/signin")
                        .ReturnsAsync(
                            (HttpRequestMessage req, CancellationToken _) =>
                                FakePlexApiData
                                    .PostUsersSignInDataResponse(HttpStatusCode.Unauthorized, seed, req)
                                    .RawResponse
                        );
                }
        );

        var plexAccount = FakeData.GetPlexAccount(4347564).Generate();
        var plexAccountDTO = plexAccount.ToDTO();

        // Act
        var response = await container.ApiClient.PostAsJsonAsync(
            ApiRoutes.PlexAccountController + "/validate",
            plexAccountDTO
        );
        var resultDTO = await response.Deserialize<PlexAccountDTO>();
        var result = resultDTO.ToResultModel();

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.Count.ShouldBe(2);
        result.Has401UnauthorizedError().ShouldBeTrue();
    }
}
