using Reaparr.Application.Contracts;
using Reaparr.BaseTests;
using Reaparr.Data.Contracts;

namespace Reaparr.Application.UnitTests;

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

        mock.SetupCommand(It.IsAny<InspectAllPlexServersByAccountIdCommand>).ReturnsAsync(Result.Ok());

        // Act
        var endPoint = SetupEndpointUnitTest<CreatePlexAccountEndpoint>();
        await endPoint.HandleAsync(
            new CreatePlexAccountEndpointRequest { PlexAccount = newAccount.ToDTO() },
            CancellationToken
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
        var plexAccount = await IDbContext.PlexAccounts.GetAsync(1, CancellationToken);
        plexAccount.ShouldNotBeNull();

        var newAccount = PlexAccount.Create(plexAccount.Username, "Password123");

        mock.SetupCommand(It.IsAny<InspectAllPlexServersByAccountIdCommand>).ReturnsAsync(Result.Ok());

        // Act
        var endPoint = SetupEndpointUnitTest<CreatePlexAccountEndpoint>();
        await endPoint.HandleAsync(
            new CreatePlexAccountEndpointRequest { PlexAccount = newAccount.ToDTO() },
            CancellationToken
        );
        var result = endPoint.Response;

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }
}
