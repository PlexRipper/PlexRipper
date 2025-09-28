using System.Net;
using FastEndpoints;
using Moq.Contrib.HttpClient;
using Reaparr.Application;
using Reaparr.Application.Contracts;

namespace Reaparr.IntegrationTests;

public class ValidateCredentialsIntegrationTests : BaseIntegrationTests
{
    public ValidateCredentialsIntegrationTests(ITestOutputHelper output)
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
                config.HttpClientOptions = (x, _) =>
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
            ValidatePlexCredentialsEndpoint,
            ValidatePlexCredentialsEndpointRequest,
            ResultDTO<ValidatePlexCredentialsResponse>
        >(
            new ValidatePlexCredentialsEndpointRequest
            {
                DisplayName = plexAccountDTO.DisplayName,
                Username = plexAccountDTO.Username,
                Password = plexAccountDTO.Password,
                VerificationCode = plexAccountDTO.VerificationCode,
            }
        );
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
                config.HttpClientOptions = (x, _) =>
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
            ValidatePlexCredentialsEndpoint,
            ValidatePlexCredentialsEndpointRequest,
            ResultDTO<ValidatePlexCredentialsResponse>
        >(
            new ValidatePlexCredentialsEndpointRequest
            {
                DisplayName = plexAccountDTO.DisplayName,
                Username = plexAccountDTO.Username,
                Password = plexAccountDTO.Password,
                VerificationCode = plexAccountDTO.VerificationCode,
            }
        );
        response.Response.IsSuccessStatusCode.ShouldBeTrue();
        var result = response.Result;

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.IsUnAuthorized.ShouldBeTrue();
        response.Response.StatusCode.ShouldNotBe(HttpStatusCode.Unauthorized);
    }
}
