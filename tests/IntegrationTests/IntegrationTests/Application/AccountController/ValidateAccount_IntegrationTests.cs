using System.Net;
using Application.Contracts;
using FastEndpoints;
using Moq.Contrib.HttpClient;
using PlexRipper.Application;

namespace IntegrationTests.AccountController;

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
        var client = container.GetApiClient();
        await client.SignIn();

        var response = await client.POSTAsync<
            ValidatePlexAccountEndpoint,
            ValidatePlexAccountEndpointRequest,
            ResultDTO<ValidatePlexAccountResponse>
        >(new ValidatePlexAccountEndpointRequest(plexAccountDTO));
        response.Response.IsSuccessStatusCode.ShouldBeTrue();
        var result = response.Result;

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnFailedResultWithErrorsButNot401_WhenGivenInValidCredentials()
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
        var client = container.GetApiClient();
        await client.SignIn();

        var response = await client.POSTAsync<
            ValidatePlexAccountEndpoint,
            ValidatePlexAccountEndpointRequest,
            ResultDTO<ValidatePlexAccountResponse>
        >(new ValidatePlexAccountEndpointRequest(plexAccountDTO));
        response.Response.IsSuccessStatusCode.ShouldBeTrue();
        var result = response.Result;

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.IsUnAuthorized.ShouldBeTrue();
        response.Response.StatusCode.ShouldNotBe(HttpStatusCode.Unauthorized);
    }
}
