namespace Reaparr.Application.UnitTests;

public class CreatePlexAccountEndpointUnitTests
    : BaseEndpointUnitTest<CreatePlexAccountEndpoint, CreatePlexAccountEndpointRequest, ResultDTO<PlexAccountDTO>>
{
    [Test]
    public async Task CreatePlexAccountAsync_ShouldSuccessResult_WhenAccountIsValid()
    {
        // Arrange
        var seed = new Seed(352);
        await SetupDatabase(seed);
        var newAccount = FakeData.GetPlexAccount(seed).Generate();

        Mock.SetupCommand(It.IsAny<InspectAllPlexServersByAccountIdCommand>).ReturnsAsync(Result.Ok());

        var createPlexAccountDTO = new CreatePlexAccountEndpointRequest
        {
            DisplayName = newAccount.DisplayName,
            Username = newAccount.Username,
            Password = newAccount.Password,
            IsEnabled = newAccount.IsEnabled,
            IsMain = newAccount.IsMain,
            IsValidated = newAccount.IsValidated,
            ValidatedAt = newAccount.ValidatedAt,
            Uuid = newAccount.Uuid,
            PlexId = newAccount.PlexId,
            Email = newAccount.Email,
            Title = newAccount.Title,
            ClientId = newAccount.ClientId,
            Is2Fa = newAccount.Is2Fa,
            CustomAuthenticationToken = newAccount.CustomAuthenticationToken,
            AuthenticationToken = newAccount.AuthenticationToken,
        };

        // Act
        var endpointResult = await TestEndpointHandleAsync(createPlexAccountDTO);

        // Assert
        endpointResult.ShouldNotBeNull();
        endpointResult.Response.ShouldNotBeNull();
        endpointResult.Response.IsSuccess.ShouldBeTrue();
        var createdAccount = await IDbContext.PlexAccounts.SingleOrDefaultAsync(
            x => x.Username == newAccount.Username,
            CancellationToken
        );
        createdAccount.ShouldNotBeNull();
        createdAccount.Username.ShouldBe(newAccount.Username);

        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<InspectAllPlexServersByAccountIdCommand>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
    }

    [Test]
    public async Task CreatePlexAccountAsync_ShouldFailedResult_WhenAccountUsernameExistenceCheckFailed()
    {
        // Arrange
        await SetupDatabase(234, config => config.PlexAccountCount = 1);
        var plexAccount = await IDbContext.PlexAccounts.GetAsync(1, CancellationToken);
        plexAccount.ShouldNotBeNull();

        var newAccount = FakeData.GetPlexAccount(234).Generate();
        var duplicateUsername = plexAccount.Username;

        Mock.SetupCommand(It.IsAny<InspectAllPlexServersByAccountIdCommand>).ReturnsAsync(Result.Ok());

        var createPlexAccountDTO = new CreatePlexAccountEndpointRequest
        {
            DisplayName = newAccount.DisplayName,
            Username = duplicateUsername,
            Password = newAccount.Password,
            IsEnabled = newAccount.IsEnabled,
            IsMain = newAccount.IsMain,
            IsValidated = newAccount.IsValidated,
            ValidatedAt = newAccount.ValidatedAt,
            Uuid = newAccount.Uuid,
            PlexId = newAccount.PlexId,
            Email = newAccount.Email,
            Title = newAccount.Title,
            ClientId = newAccount.ClientId,
            Is2Fa = newAccount.Is2Fa,
            CustomAuthenticationToken = newAccount.CustomAuthenticationToken,
            AuthenticationToken = newAccount.AuthenticationToken,
        };

        // Act
        var endpointResult = await TestEndpointHandleAsync(createPlexAccountDTO);

        // Assert
        endpointResult.ShouldNotBeNull();
        endpointResult.Response.ShouldNotBeNull();
        endpointResult.Response.IsSuccess.ShouldBeFalse();
        var duplicateAccounts = await IDbContext.PlexAccounts.CountAsync(
            x => x.Username == duplicateUsername,
            CancellationToken
        );
        duplicateAccounts.ShouldBe(1);

        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<InspectAllPlexServersByAccountIdCommand>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
    }
}
