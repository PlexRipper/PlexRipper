using Reaparr.Application.Contracts;
using Reaparr.FluentResultExtensions;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.Application.UnitTests;

public class ValidatePlexTokenEndpointUnitTests : BaseUnitTest
{
    public ValidatePlexTokenEndpointUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldValidateThePlexToken_WhenTokenIsValid()
    {
        // Arrange
        var seed = new Seed(2058);
        var testAccountDTO = FakeData.GetPlexAccount(seed).Generate().ToDTO();
        testAccountDTO.Username = "testuser";
        testAccountDTO.Password = string.Empty;
        testAccountDTO.CustomAuthenticationToken = "valid-token";

        var testAccountResponse = testAccountDTO.ToModel();
        UpdateInitProperty(testAccountResponse, nameof(testAccountResponse.ValidatedAt), DateTime.UtcNow);

        var commandResult = new ValidatePlexTokenCommandResult()
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
        };

        mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<ValidatePlexTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(commandResult));

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
        value.IsValidated.ShouldBeTrue();
        value.ValidatedAt.ShouldNotBeNull();
        value.ValidatedAt?.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        value.Is2Fa.ShouldBeFalse();
        value.Email.ShouldBe(testAccountDTO.Email);
        value.Username.ShouldBe(testAccountDTO.Username);
        value.AuthenticationToken.ShouldNotBeEmpty();
        value.ClientId.ShouldBe(testAccountResponse.ClientId);
        value.Title.ShouldBe(testAccountResponse.Title);
        value.PlexId.ShouldBe(testAccountResponse.PlexId);
        value.Uuid.ShouldBe(testAccountResponse.Uuid);

        mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<ValidatePlexTokenCommand>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task ShouldMarkUnauthorized_WhenThePlexAPIRespondsWithA401()
    {
        // Arrange
        var seed = new Seed(203958);
        var testAccountDTO = FakeData.GetPlexAccount(seed).Generate().ToDTO();
        testAccountDTO.Username = "testuser";
        testAccountDTO.Password = string.Empty;
        testAccountDTO.CustomAuthenticationToken = "invalid-token";

        var commandResult = new ValidatePlexTokenCommandResult
        {
            ClientId = testAccountDTO.ClientId,
            Username = testAccountDTO.Username,
            Email = testAccountDTO.Email,
            Title = testAccountDTO.Title,
            PlexId = testAccountDTO.PlexId,
            Uuid = testAccountDTO.Uuid,
            AuthenticationToken = testAccountDTO.AuthenticationToken,
            IsValidated = false,
            ValidatedAt = null,
            Is2Fa = false,
        };

        mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<ValidatePlexTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(commandResult).AddPlex401UnauthorizedError());

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
        value.IsUnAuthorized.ShouldBeTrue();
        value.IsValidated.ShouldBeFalse();
        value.ValidatedAt.ShouldBeNull();
        value.Is2Fa.ShouldBeFalse();
        value.Username.ShouldBeEmpty();
        value.Email.ShouldBeEmpty();
        value.ClientId.ShouldBeEmpty();
        value.Title.ShouldBeEmpty();
        value.PlexId.ShouldBe(0);
        value.Uuid.ShouldBeEmpty();
        value.AuthenticationToken.ShouldBeEmpty();

        mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<ValidatePlexTokenCommand>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task ShouldHandle2FaEnabledScenario_WhenTokenIsValid()
    {
        // Arrange
        var seed = new Seed(2059);
        var testAccountDTO = FakeData.GetPlexAccount(seed).Generate().ToDTO();
        testAccountDTO.Username = "testuser2fa";
        testAccountDTO.Password = string.Empty;
        testAccountDTO.CustomAuthenticationToken = "valid-2fa-token";

        var commandResult = new ValidatePlexTokenCommandResult()
        {
            ClientId = testAccountDTO.ClientId,
            Username = testAccountDTO.Username,
            Email = testAccountDTO.Email,
            Title = testAccountDTO.Title,
            PlexId = testAccountDTO.PlexId,
            Uuid = testAccountDTO.Uuid,
            AuthenticationToken = testAccountDTO.AuthenticationToken,
            IsValidated = true,
            ValidatedAt = DateTime.UtcNow,
            Is2Fa = true,
        };

        mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<ValidatePlexTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(commandResult));

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
        value.IsValidated.ShouldBeTrue();
        value.ValidatedAt.ShouldNotBeNull();
        value.ValidatedAt?.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        value.Is2Fa.ShouldBeTrue();
        value.Email.ShouldBe(testAccountDTO.Email);
        value.Username.ShouldBe(testAccountDTO.Username);
        value.AuthenticationToken.ShouldNotBeEmpty();

        mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<ValidatePlexTokenCommand>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task ShouldNotMarkUnauthorized_WhenUnhandledPlexErrorsOccur()
    {
        // Arrange
        var seed = new Seed(203960);
        var testAccountDTO = FakeData.GetPlexAccount(seed).Generate().ToDTO();
        testAccountDTO.Username = "testuser";
        testAccountDTO.Password = string.Empty;
        testAccountDTO.CustomAuthenticationToken = "error-token";

        var commandResult = new ValidatePlexTokenCommandResult
        {
            ClientId = testAccountDTO.ClientId,
            Username = testAccountDTO.Username,
            Email = testAccountDTO.Email,
            Title = testAccountDTO.Title,
            PlexId = testAccountDTO.PlexId,
            Uuid = testAccountDTO.Uuid,
            AuthenticationToken = testAccountDTO.AuthenticationToken,
            IsValidated = false,
            ValidatedAt = null,
            Is2Fa = false,
        };

        mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<ValidatePlexTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result
                    .Ok(commandResult)
                    .WithErrors([new PlexError("Error #1") { Code = 1234 }, new PlexError("Error #2") { Code = 5678 }])
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
        var result = ep.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.Count.ShouldBe(2);

        mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<ValidatePlexTokenCommand>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenCommandExecutionFails()
    {
        // Arrange
        var seed = new Seed(203961);
        var testAccountDTO = FakeData.GetPlexAccount(seed).Generate().ToDTO();
        testAccountDTO.Username = "testuser";
        testAccountDTO.Password = string.Empty;
        testAccountDTO.CustomAuthenticationToken = "failing-token";

        mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<ValidatePlexTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Command execution failed"));

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
        var result = ep.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();

        mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<ValidatePlexTokenCommand>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task ShouldHandleEmptyDisplayName_WhenDisplayNameIsNotProvided()
    {
        // Arrange
        var seed = new Seed(2058);
        var testAccountDTO = FakeData.GetPlexAccount(seed).Generate().ToDTO();
        testAccountDTO.Username = "testuser";
        testAccountDTO.Password = string.Empty;
        testAccountDTO.CustomAuthenticationToken = "valid-token";

        var commandResult = new ValidatePlexTokenCommandResult()
        {
            ClientId = testAccountDTO.ClientId,
            Username = testAccountDTO.Username,
            Email = testAccountDTO.Email,
            Title = testAccountDTO.Title,
            PlexId = testAccountDTO.PlexId,
            Uuid = testAccountDTO.Uuid,
            AuthenticationToken = testAccountDTO.AuthenticationToken,
            IsValidated = true,
            ValidatedAt = DateTime.UtcNow,
            Is2Fa = false,
        };

        mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<ValidatePlexTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(commandResult));

        // Act
        var ep = SetupEndpointUnitTest<ValidatePlexTokenEndpoint>();
        await ep.HandleAsync(
            new ValidatePlexTokenEndpointRequest()
            {
                DisplayName = "UnknownDisplayName", // Default value
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
        value.IsValidated.ShouldBeTrue();

        mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<ValidatePlexTokenCommand>(), It.IsAny<CancellationToken>()), Times.Once());
    }
}
