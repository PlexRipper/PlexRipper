using Reaparr.Application.Contracts;
using Reaparr.FluentResultExtensions;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.Application.UnitTests;

public class ValidateTokenEndpointEndpointUnitTests : BaseUnitTest
{
    public ValidateTokenEndpointEndpointUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldValidateThePlexToken_WhenTokenIsValid()
    {
        // Arrange
        var seed = new Seed(2058);
        var testAccountDTO = FakeData.GetPlexAccount(seed).Generate().ToDTO();
        testAccountDTO.Username = string.Empty;
        testAccountDTO.Password = string.Empty;
        testAccountDTO.CustomAuthenticationToken = "valid-token";

        var testAccountResponse = testAccountDTO.ToModel();
        UpdateInitProperty(testAccountResponse, nameof(testAccountResponse.ValidatedAt), DateTime.UtcNow);

        mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<ValidatePlexTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Ok(
                    new ValidatePlexTokenCommandResult()
                    {
                        ClientId = testAccountResponse.ClientId,
                        Username = testAccountResponse.Username,
                        Email = testAccountResponse.Email,
                        Title = testAccountResponse.Title,
                        PlexId = testAccountResponse.PlexId,
                        Uuid = testAccountResponse.Uuid,
                        AuthenticationToken = testAccountResponse.AuthenticationToken,
                        IsValidated = true,
                        ValidatedAt = DateTime.UtcNow,
                        Is2Fa = false,
                    }
                )
            );

        // Act
        var ep = SetupEndpointUnitTest<ValidatePlexTokenEndpoint>();
        await ep.HandleAsync(
            new ValidatePlexTokenEndpointRequest()
            {
                DisplayName = testAccountDTO.DisplayName,
                ManualAuthenticationToken = testAccountDTO.AuthenticationToken,
            },
            CancellationToken
        );
        var result = ep.Response as ResultDTO<ValidatePlexTokenEndpointResponse>;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.ShouldNotBeNull();
        value.IsUnAuthorized.ShouldBeFalse();
        value.ShouldNotBeNull();
        value.IsValidated.ShouldBeTrue();
        value.ValidatedAt.ShouldNotBeNull();
        value.ValidatedAt?.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        value.Is2Fa.ShouldBeFalse();
        value.Email.ShouldBe(testAccountDTO.Email);
        value.Username.ShouldBe(testAccountDTO.Username);
        value.AuthenticationToken.ShouldNotBeEmpty();
    }
}
