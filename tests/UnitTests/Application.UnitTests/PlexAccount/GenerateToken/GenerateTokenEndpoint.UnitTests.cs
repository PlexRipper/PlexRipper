using Application.Contracts;
using FluentResultExtensions;
using Microsoft.EntityFrameworkCore;
using PlexApi.Contracts;

namespace PlexRipper.Application.UnitTests;

public class GenerateTokenEndpointUnitTests : BaseUnitTest
{
    public GenerateTokenEndpointUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldGenerateToken_WhenSignInIsSuccessful()
    {
        // Arrange
        var seed = await SetupDatabase(2305, config => config.PlexAccountCount = 1);
        var plexAccount = await IDbContext.PlexAccounts.FirstAsync(CancellationToken);

        mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<PlexSignInCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(FakeData.GetPlexAccount(seed).Generate()));

        // Act
        var ep = SetupEndpointUnitTest<GeneratePlexTokenEndpoint>();
        await ep.HandleAsync(
            new GeneratePlexTokenEndpointRequest { PlexAccountId = plexAccount.Id, VerificationCode = "" },
            CancellationToken
        );
        var result = ep.Response as ResultDTO<GeneratePlexTokenResponse>;

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

    [Fact]
    public async Task ShouldReturnAVerificationCodeResponse_WhenThePlexAPIRespondsWithA2faResponse()
    {
        // Arrange
        await SetupDatabase(232432, config => config.PlexAccountCount = 1);
        var plexAccount = await IDbContext.PlexAccounts.FirstAsync(CancellationToken);

        mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<PlexSignInCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Fail(new PlexError("Enter verification code") { Code = PlexErrorCodes.EnterVerificationCode })
            );

        // Act
        var ep = SetupEndpointUnitTest<GeneratePlexTokenEndpoint>();
        await ep.HandleAsync(
            new GeneratePlexTokenEndpointRequest { PlexAccountId = plexAccount.Id },
            CancellationToken
        );
        var result = ep.Response as ResultDTO<GeneratePlexTokenResponse>;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.ShouldNotBeNull();
        value.IsUnAuthorized.ShouldBeFalse();
        value.NeedsVerificationCode.ShouldBeTrue();
        value.PlexAuthToken.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldNotReturnA401Response_WhenThePlexAPIRespondsWithA401Response()
    {
        // Arrange
        await SetupDatabase(433222, config => config.PlexAccountCount = 1);
        var plexAccount = await IDbContext.PlexAccounts.FirstAsync(CancellationToken);

        mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<PlexSignInCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(new PlexError("Unauthorized")).Add401UnauthorizedError());

        // Act
        var ep = SetupEndpointUnitTest<GeneratePlexTokenEndpoint>();
        await ep.HandleAsync(
            new GeneratePlexTokenEndpointRequest { PlexAccountId = plexAccount.Id },
            CancellationToken
        );
        var result = ep.Response as ResultDTO<GeneratePlexTokenResponse>;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.ShouldNotBeNull();
        value.IsUnAuthorized.ShouldBeTrue();
        value.NeedsVerificationCode.ShouldBeFalse();
        value.PlexAuthToken.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldReturnAllPlexErrors_WhenDefaultingOnErrorHandling()
    {
        // Arrange
        await SetupDatabase(433222, config => config.PlexAccountCount = 1);
        var plexAccount = await IDbContext.PlexAccounts.FirstAsync(CancellationToken);

        mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<PlexSignInCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result
                    .Fail(new PlexError("Unauthorized"))
                    .WithErrors([new PlexError("Error #1") { Code = 1234 }, new PlexError("Error #2") { Code = 5678 }])
            );

        // Act
        var ep = SetupEndpointUnitTest<GeneratePlexTokenEndpoint>();
        await ep.HandleAsync(
            new GeneratePlexTokenEndpointRequest { PlexAccountId = plexAccount.Id },
            CancellationToken
        );
        var result = ep.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();

        result.Errors.Count.ShouldBe(3);
    }
}
