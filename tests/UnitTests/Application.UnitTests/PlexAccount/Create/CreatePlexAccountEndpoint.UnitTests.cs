namespace Reaparr.Application.UnitTests;

public class CreatePlexAccountEndpointUnitTests : BaseEndpointUnitTest<CreatePlexAccountEndpoint, CreatePlexAccountEndpointRequest, ResultDTO<PlexAccountDTO>>
{
    [Test]
    public async Task CreatePlexAccountAsync_ShouldSuccessResult_WhenAccountIsValid()
    {
        // Arrange
        await SetupDatabase(352);
        var newAccount = PlexAccount.Create("TestUsername", "Password123");

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
        var result = endpointResult.Response;

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Test]
    public async Task CreatePlexAccountAsync_ShouldFailedResult_WhenAccountUsernameExistenceCheckFailed()
    {
        // Arrange
        await SetupDatabase(234, config => config.PlexAccountCount = 1);
        var plexAccount = await IDbContext.PlexAccounts.GetAsync(1, CancellationToken);
        plexAccount.ShouldNotBeNull();

        var newAccount = PlexAccount.Create(plexAccount.Username, "Password123");

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
        var result = endpointResult.Response;

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }
}
