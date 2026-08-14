namespace Reaparr.Application.UnitTests;

public class GenerateTokenEndpointUnitTests
    : BaseEndpointUnitTest<
        GeneratePlexTokenEndpoint,
        GeneratePlexTokenEndpointRequest,
        ResultDTO<GeneratePlexTokenResponse>
    >
{
    [Test]
    public async Task ShouldGenerateToken_WhenSignInIsSuccessful()
    {
        // Arrange
        var seed = await SetupDatabase(2305, config => config.PlexAccountCount = 1);
        var plexAccount = await IDbContext.PlexAccounts.FirstAsync(CancellationToken);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<PlexSignInCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                var x = FakeData.GetPlexAccount(seed).Generate();
                return Result.Ok(
                    new PlexSignInCommandResult
                    {
                        ClientId = x.ClientId,
                        Username = x.Username,
                        Password = x.Password,
                        Email = x.Email,
                        Title = x.Title,
                        PlexId = x.PlexId,
                        Uuid = x.Uuid,
                        AuthenticationToken = x.AuthenticationToken,
                        IsValidated = x.IsValidated,
                        ValidatedAt = x.ValidatedAt,
                        Is2Fa = x.Is2Fa,
                    }
                );
            });

        // Act
        var endpointResult = await TestEndpointHandleAsync(
            new GeneratePlexTokenEndpointRequest { PlexAccountId = plexAccount.Id, VerificationCode = "" }
        );
        var result = endpointResult.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.ShouldNotBeNull();
        value.IsUnAuthorized.ShouldBeFalse();
        var authToken = value.PlexAuthToken;
        authToken.ShouldNotBeEmpty();
        authToken.ShouldNotBe(plexAccount.AuthenticationToken);
    }

    [Test]
    public async Task ShouldReturnAVerificationCodeResponse_WhenThePlexAPIRespondsWithA2faResponse()
    {
        // Arrange
        await SetupDatabase(232432, config => config.PlexAccountCount = 1);
        var plexAccount = await IDbContext.PlexAccounts.FirstAsync(CancellationToken);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<PlexSignInCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Fail(new PlexError("Enter verification code") { Code = PlexErrorCodes.EnterVerificationCode })
            );

        // Act
        var endpointResult = await TestEndpointHandleAsync(
            new GeneratePlexTokenEndpointRequest { PlexAccountId = plexAccount.Id }
        );
        var result = endpointResult.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.ShouldNotBeNull();
        value.IsUnAuthorized.ShouldBeFalse();
        value.NeedsVerificationCode.ShouldBeTrue();
        value.PlexAuthToken.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldNotReturnA401Response_WhenThePlexAPIRespondsWithA401Response()
    {
        // Arrange
        await SetupDatabase(433222, config => config.PlexAccountCount = 1);
        var plexAccount = await IDbContext.PlexAccounts.FirstAsync(CancellationToken);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<PlexSignInCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(new PlexError("Unauthorized")).Add401UnauthorizedError());

        // Act
        var endpointResult = await TestEndpointHandleAsync(
            new GeneratePlexTokenEndpointRequest { PlexAccountId = plexAccount.Id }
        );
        var result = endpointResult.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.ShouldNotBeNull();
        value.IsUnAuthorized.ShouldBeTrue();
        value.NeedsVerificationCode.ShouldBeFalse();
        value.PlexAuthToken.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldReturnAllPlexErrors_WhenDefaultingOnErrorHandling()
    {
        // Arrange
        await SetupDatabase(433222, config => config.PlexAccountCount = 1);
        var plexAccount = await IDbContext.PlexAccounts.FirstAsync(CancellationToken);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<PlexSignInCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result
                    .Fail(new PlexError("Unauthorized"))
                    .WithErrors([new PlexError("Error #1") { Code = 1234 }, new PlexError("Error #2") { Code = 5678 }])
            );

        // Act
        var endpointResult = await TestEndpointHandleAsync(
            new GeneratePlexTokenEndpointRequest { PlexAccountId = plexAccount.Id }
        );
        var result = endpointResult.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();

        result.Errors.Count.ShouldBe(3);
    }
}
