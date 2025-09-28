using Reaparr.Application.Contracts;
using Reaparr.FluentResultExtensions;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.Application.UnitTests;

public class ValidatePlexCredentialsEndpointUnitTests : BaseUnitTest
{
    public ValidatePlexCredentialsEndpointUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldValidateTheAccount_WhenNo2FaIsEnabled()
    {
        // Arrange
        var seed = new Seed(2058);
        var testAccountDTO = FakeData.GetPlexAccount(seed).Generate().ToDTO();

        var testAccountResponse = testAccountDTO.ToModel();
        UpdateInitProperty(testAccountResponse, nameof(testAccountResponse.ValidatedAt), DateTime.UtcNow);

        mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<PlexSignInCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Ok(
                    new PlexSignInCommandResult
                    {
                        ClientId = testAccountResponse.ClientId,
                        Username = testAccountResponse.Username,
                        Password = testAccountResponse.Password,
                        Email = testAccountResponse.Email,
                        Title = testAccountResponse.Title,
                        PlexId = testAccountResponse.PlexId,
                        Uuid = testAccountResponse.Uuid,
                        AuthenticationToken = testAccountResponse.AuthenticationToken,
                        IsValidated = testAccountResponse.IsValidated,
                        ValidatedAt = testAccountResponse.ValidatedAt,
                        Is2Fa = testAccountResponse.Is2Fa,
                    }
                )
            );

        // Act
        var ep = SetupEndpointUnitTest<ValidatePlexCredentialsEndpoint>();
        await ep.HandleAsync(
            new ValidatePlexCredentialsEndpointRequest()
            {
                DisplayName = testAccountDTO.DisplayName,
                Username = testAccountDTO.Username,
                Password = testAccountDTO.Password,
                VerificationCode = testAccountDTO.VerificationCode,
            },
            CancellationToken
        );
        var result = ep.Response as ResultDTO<ValidatePlexCredentialsResponse>;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.ShouldNotBeNull();
        value.IsUnAuthorized.ShouldBeFalse();
        var account = value;
        account.ShouldNotBeNull();
        account.IsValidated.ShouldBeTrue();
        account.ValidatedAt.ShouldNotBeNull();
        account.ValidatedAt?.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        account.Is2Fa.ShouldBeFalse();
        account.Email.ShouldBe(testAccountDTO.Email);
        account.Username.ShouldBe(testAccountDTO.Username);
        account.Password.ShouldBe(testAccountDTO.Password);
        account.AuthenticationToken.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task ShouldReturnAVerificationCodeResponse_WhenThePlexAPIRespondsWithA2faResponse()
    {
        // Arrange
        var seed = new Seed(203958);
        var testAccountDTO = FakeData.GetPlexAccount(seed).Generate().ToDTO();
        testAccountDTO.IsValidated = false;
        testAccountDTO.ValidatedAt = null;
        testAccountDTO.CustomAuthenticationToken = string.Empty;

        mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<PlexSignInCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Fail(new PlexError("Enter verification code") { Code = PlexErrorCodes.EnterVerificationCode })
            );

        // Act
        var ep = SetupEndpointUnitTest<ValidatePlexCredentialsEndpoint>();
        await ep.HandleAsync(
            new ValidatePlexCredentialsEndpointRequest()
            {
                DisplayName = testAccountDTO.DisplayName,
                Username = testAccountDTO.Username,
                Password = testAccountDTO.Password,
                VerificationCode = testAccountDTO.VerificationCode,
            },
            CancellationToken
        );
        var result = ep.Response as ResultDTO<ValidatePlexCredentialsResponse>;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.ShouldNotBeNull();
        value.IsUnAuthorized.ShouldBeFalse();
        var account = value;
        account.ShouldNotBeNull();
        account.IsValidated.ShouldBeFalse();
        account.ValidatedAt.ShouldBe(null);
        account.Is2Fa.ShouldBeTrue();
        account.Email.ShouldBe(testAccountDTO.Email);
        account.Username.ShouldBe(testAccountDTO.Username);
        account.Password.ShouldBe(testAccountDTO.Password);
        account.AuthenticationToken.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task ShouldNotReturnA401Response_WhenThePlexAPIRespondsWithA401Response()
    {
        // Arrange
        var seed = new Seed(203958);
        var testAccountDTO = FakeData.GetPlexAccount(seed).Generate().ToDTO();
        testAccountDTO.IsValidated = false;
        testAccountDTO.ValidatedAt = null;
        testAccountDTO.CustomAuthenticationToken = string.Empty;

        mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<PlexSignInCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(new PlexError("Unauthorized")).AddPlex401UnauthorizedError());

        // Act
        var ep = SetupEndpointUnitTest<ValidatePlexCredentialsEndpoint>();
        await ep.HandleAsync(
            new ValidatePlexCredentialsEndpointRequest()
            {
                DisplayName = testAccountDTO.DisplayName,
                Username = testAccountDTO.Username,
                Password = testAccountDTO.Password,
                VerificationCode = testAccountDTO.VerificationCode,
            },
            CancellationToken
        );
        var result = ep.Response as ResultDTO<ValidatePlexCredentialsResponse>;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.ShouldNotBeNull();
        value.IsUnAuthorized.ShouldBeTrue();
        var account = value;
        account.ShouldNotBeNull();
        account.IsValidated.ShouldBeFalse();
        account.ValidatedAt.ShouldBe(null);
        account.Is2Fa.ShouldBeFalse();
        account.Email.ShouldBe(testAccountDTO.Email);
        account.Username.ShouldBe(testAccountDTO.Username);
        account.Password.ShouldBe(testAccountDTO.Password);
        account.AuthenticationToken.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task ShouldNotReturnA401Response_WhenThePlexAPIRespondsWithA401ResponseAndIsInAuthTokenMode()
    {
        // Arrange
        var seed = new Seed(203958);
        var testAccountDTO = FakeData.GetPlexAccount(seed).Generate().ToDTO();
        testAccountDTO.IsValidated = false;
        testAccountDTO.ValidatedAt = null;
        testAccountDTO.Username = string.Empty;
        testAccountDTO.Password = string.Empty;
        testAccountDTO.CustomAuthenticationToken = "valid-token";

        mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<ValidatePlexTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(new PlexError("Unauthorized")).AddPlex401UnauthorizedError());

        // Act
        var ep = SetupEndpointUnitTest<ValidatePlexCredentialsEndpoint>();
        await ep.HandleAsync(
            new ValidatePlexCredentialsEndpointRequest()
            {
                DisplayName = testAccountDTO.DisplayName,
                Username = testAccountDTO.Username,
                Password = testAccountDTO.Password,
                VerificationCode = testAccountDTO.VerificationCode,
            },
            CancellationToken
        );
        var result = ep.Response as ResultDTO<ValidatePlexCredentialsResponse>;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.ShouldNotBeNull();
        value.IsUnAuthorized.ShouldBeTrue();
        var account = value;
        account.ShouldNotBeNull();
        account.IsValidated.ShouldBeFalse();
        account.ValidatedAt.ShouldBe(null);
        account.Is2Fa.ShouldBeFalse();
        account.Email.ShouldBe(testAccountDTO.Email);
        account.Username.ShouldBe(testAccountDTO.Username);
        account.Password.ShouldBe(testAccountDTO.Password);
        account.AuthenticationToken.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task ShouldReturnAllPlexErrors_WhenDefaultingOnErrorHandling()
    {
        // Arrange
        var seed = new Seed(203958);
        var testAccountDTO = FakeData.GetPlexAccount(seed).Generate().ToDTO();
        testAccountDTO.IsValidated = false;
        testAccountDTO.ValidatedAt = null;
        testAccountDTO.CustomAuthenticationToken = string.Empty;

        mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<PlexSignInCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result
                    .Fail(new PlexError("Unauthorized"))
                    .WithErrors([new PlexError("Error #1") { Code = 1234 }, new PlexError("Error #2") { Code = 5678 }])
            );

        // Act
        var ep = SetupEndpointUnitTest<ValidatePlexCredentialsEndpoint>();
        await ep.HandleAsync(
            new ValidatePlexCredentialsEndpointRequest()
            {
                DisplayName = testAccountDTO.DisplayName,
                Username = testAccountDTO.Username,
                Password = testAccountDTO.Password,
                VerificationCode = testAccountDTO.VerificationCode,
            },
            CancellationToken
        );
        var result = ep.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();

        result.Errors.Count.ShouldBe(3);
    }
}
