using Application.Contracts;
using FluentResultExtensions;
using PlexApi.Contracts;

namespace PlexRipper.Application.UnitTests;

public class ValidatePlexAccountEndpointUnitTests : BaseUnitTest
{
    public ValidatePlexAccountEndpointUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldValidateTheAccount_WhenNo2FaIsEnabled()
    {
        // Arrange
        var seed = new Seed(2058);
        var testAccountDTO = FakeData.GetPlexAccount(seed).Generate().ToDTO();

        var testAccountResponse = testAccountDTO.ToModel();
        UpdateInitProperty(testAccountResponse, nameof(testAccountResponse.ValidatedAt), DateTime.UtcNow);

        mock.Mock<IPlexApiService>()
            .Setup(x => x.PlexSignInAsync(It.IsAny<PlexAccount>()))
            .ReturnsAsync(Result.Ok(testAccountResponse));

        // Act
        var ep = SetupEndpointUnitTest<ValidatePlexAccountEndpoint>();
        await ep.HandleAsync(new ValidatePlexAccountEndpointRequest(testAccountDTO), CancellationToken.None);
        var result = ep.Response as ResultDTO<ValidatePlexAccountResponse>;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.ShouldNotBeNull();
        value.IsUnAuthorized.ShouldBeFalse();
        var account = value.PlexAccountDTO;
        account.ShouldNotBeNull();
        account.IsValidated.ShouldBeTrue();
        account.ValidatedAt.ShouldNotBeNull();
        account.ValidatedAt?.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        account.Is2Fa.ShouldBeFalse();
        account.Email.ShouldBe(testAccountDTO.Email);
        account.Username.ShouldBe(testAccountDTO.Username);
        account.Password.ShouldBe(testAccountDTO.Password);
        account.CustomAuthenticationToken.ShouldBeEmpty();
    }

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

        mock.Mock<IPlexApiService>()
            .Setup(x => x.ValidatePlexToken(It.IsAny<PlexAccount>()))
            .ReturnsAsync(Result.Ok(testAccountResponse));

        // Act
        var ep = SetupEndpointUnitTest<ValidatePlexAccountEndpoint>();
        await ep.HandleAsync(new ValidatePlexAccountEndpointRequest(testAccountDTO), CancellationToken.None);
        var result = ep.Response as ResultDTO<ValidatePlexAccountResponse>;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.ShouldNotBeNull();
        value.IsUnAuthorized.ShouldBeFalse();
        var account = value.PlexAccountDTO;
        account.ShouldNotBeNull();
        account.IsValidated.ShouldBeTrue();
        account.ValidatedAt.ShouldNotBeNull();
        account.ValidatedAt?.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        account.Is2Fa.ShouldBeFalse();
        account.Email.ShouldBe(testAccountDTO.Email);
        account.Username.ShouldBe(testAccountDTO.Username);
        account.Password.ShouldBe(testAccountDTO.Password);
        account.CustomAuthenticationToken.ShouldNotBeEmpty();
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

        mock.Mock<IPlexApiService>()
            .Setup(x => x.PlexSignInAsync(It.IsAny<PlexAccount>()))
            .ReturnsAsync(
                Result.Fail(new PlexError("Enter verification code") { Code = PlexErrorCodes.EnterVerificationCode })
            );

        // Act
        var ep = SetupEndpointUnitTest<ValidatePlexAccountEndpoint>();
        await ep.HandleAsync(new ValidatePlexAccountEndpointRequest(testAccountDTO), CancellationToken.None);
        var result = ep.Response as ResultDTO<ValidatePlexAccountResponse>;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.ShouldNotBeNull();
        value.IsUnAuthorized.ShouldBeFalse();
        var account = value.PlexAccountDTO;
        account.ShouldNotBeNull();
        account.IsValidated.ShouldBeFalse();
        account.ValidatedAt.ShouldBe(null);
        account.Is2Fa.ShouldBeTrue();
        account.Email.ShouldBe(testAccountDTO.Email);
        account.Username.ShouldBe(testAccountDTO.Username);
        account.Password.ShouldBe(testAccountDTO.Password);
        account.CustomAuthenticationToken.ShouldBeEmpty();
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

        mock.Mock<IPlexApiService>()
            .Setup(x => x.PlexSignInAsync(It.IsAny<PlexAccount>()))
            .ReturnsAsync(Result.Fail(new PlexError("Unauthorized")).AddPlex401UnauthorizedError());

        // Act
        var ep = SetupEndpointUnitTest<ValidatePlexAccountEndpoint>();
        await ep.HandleAsync(new ValidatePlexAccountEndpointRequest(testAccountDTO), CancellationToken.None);
        var result = ep.Response as ResultDTO<ValidatePlexAccountResponse>;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.ShouldNotBeNull();
        value.IsUnAuthorized.ShouldBeTrue();
        var account = value.PlexAccountDTO;
        account.ShouldNotBeNull();
        account.IsValidated.ShouldBeFalse();
        account.ValidatedAt.ShouldBe(null);
        account.Is2Fa.ShouldBeFalse();
        account.Email.ShouldBe(testAccountDTO.Email);
        account.Username.ShouldBe(testAccountDTO.Username);
        account.Password.ShouldBe(testAccountDTO.Password);
        account.CustomAuthenticationToken.ShouldBeEmpty();
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

        mock.Mock<IPlexApiService>()
            .Setup(x => x.ValidatePlexToken(It.IsAny<PlexAccount>()))
            .ReturnsAsync(Result.Fail(new PlexError("Unauthorized")).AddPlex401UnauthorizedError());

        // Act
        var ep = SetupEndpointUnitTest<ValidatePlexAccountEndpoint>();
        await ep.HandleAsync(new ValidatePlexAccountEndpointRequest(testAccountDTO), CancellationToken.None);
        var result = ep.Response as ResultDTO<ValidatePlexAccountResponse>;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.ShouldNotBeNull();
        value.IsUnAuthorized.ShouldBeTrue();
        var account = value.PlexAccountDTO;
        account.ShouldNotBeNull();
        account.IsValidated.ShouldBeFalse();
        account.ValidatedAt.ShouldBe(null);
        account.Is2Fa.ShouldBeFalse();
        account.Email.ShouldBe(testAccountDTO.Email);
        account.Username.ShouldBe(testAccountDTO.Username);
        account.Password.ShouldBe(testAccountDTO.Password);
        account.CustomAuthenticationToken.ShouldNotBeEmpty();
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

        mock.Mock<IPlexApiService>()
            .Setup(x => x.PlexSignInAsync(It.IsAny<PlexAccount>()))
            .ReturnsAsync(
                Result
                    .Fail(new PlexError("Unauthorized"))
                    .WithErrors([new PlexError("Error #1") { Code = 1234 }, new PlexError("Error #2") { Code = 5678 }])
            );

        // Act
        var ep = SetupEndpointUnitTest<ValidatePlexAccountEndpoint>();
        await ep.HandleAsync(new ValidatePlexAccountEndpointRequest(testAccountDTO), CancellationToken.None);
        var result = ep.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();

        result.Errors.Count.ShouldBe(3);
    }
}
