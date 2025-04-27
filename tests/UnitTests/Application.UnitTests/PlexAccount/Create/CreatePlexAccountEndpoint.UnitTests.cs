using Application.Contracts;
using Data.Contracts;

namespace PlexRipper.Application.UnitTests;

public class CreatePlexAccountEndpointUnitTests : BaseUnitTest
{
    public CreatePlexAccountEndpointUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task CreatePlexAccountAsync_ShouldSuccessResult_WhenAccountIsValid()
    {
        // Arrange
        await SetupDatabase(352);
        var newAccount = PlexAccount.Create("TestUsername", "Password123");

        mock.SetupMediator(It.IsAny<InspectAllPlexServersByAccountIdCommand>).ReturnsAsync(Result.Ok());

        // Act
        var endPoint = SetupEndpointUnitTest<CreatePlexAccountEndpoint>();
        await endPoint.HandleAsync(
            new CreatePlexAccountEndpointRequest { PlexAccount = newAccount.ToDTO() },
            CancellationToken.None
        );
        var result = endPoint.Response;

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task CreatePlexAccountAsync_ShouldFailedResult_WhenAccountUsernameExistenceCheckFailed()
    {
        // Arrange
        await SetupDatabase(234, config => config.PlexAccountCount = 1);
        var plexAccount = await IDbContext.PlexAccounts.GetAsync(1);
        plexAccount.ShouldNotBeNull();

        var newAccount = PlexAccount.Create(plexAccount.Username, "Password123");

        mock.SetupMediator(It.IsAny<InspectAllPlexServersByAccountIdCommand>).ReturnsAsync(Result.Ok());

        // Act
        var endPoint = SetupEndpointUnitTest<CreatePlexAccountEndpoint>();
        await endPoint.HandleAsync(
            new CreatePlexAccountEndpointRequest { PlexAccount = newAccount.ToDTO() },
            CancellationToken.None
        );
        var result = endPoint.Response;

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }
}
