namespace PlexRipper.Application.UnitTests;

public class AddOrUpdatePlexAccountServersCommandHandler_UnitTests : BaseUnitTest
{
    public AddOrUpdatePlexAccountServersCommandHandler_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldAddPlexAccountServerAssociations_WhenNoneExistsYet()
    {
        // Arrange
        Seed = 45832543;
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexAccountCount = 5;
        });

        var plexAccount = IDbContext.PlexAccounts.FirstOrDefault();
        plexAccount.ShouldNotBeNull();
        var plexServers = IDbContext.PlexServers.ToList();
        var serverAccessTokens = FakeData.GetServerAccessTokenDTO(plexAccount, plexServers, Seed);

        // Remove all associations
        await IDbContext.PlexAccountServers.ExecuteDeleteAsync();

        // Act
        var request = new AddOrUpdatePlexAccountServersCommand(plexAccount.Id, serverAccessTokens);
        var handler = new AddOrUpdatePlexAccountServersCommandHandler(Log, IDbContext);
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var plexAccountServers = IDbContext.PlexAccountServers.Include(x => x.PlexServer).ToList();
        plexAccountServers.Count.ShouldBe(serverAccessTokens.Count);

        foreach (var serverAccessToken in serverAccessTokens)
            plexAccountServers
                .Any(x =>
                    x.PlexServer.MachineIdentifier == serverAccessToken.MachineIdentifier
                    && x.AuthToken == serverAccessToken.AccessToken
                    && x.PlexAccountId == plexAccount.Id
                )
                .ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldUpdateAndDeletePlexAccountServerAssociations_WhenTheyAreNotGiven()
    {
        // Arrange
        Seed = 194732;
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 5;
            config.PlexAccountCount = 1;
        });

        var plexAccount = IDbContext.PlexAccounts.FirstOrDefault();
        var plexServers = IDbContext.PlexServers.ToList();

        plexAccount.ShouldNotBeNull();

        var serverAccessTokens = FakeData.GetServerAccessTokenDTO(plexAccount, plexServers, Seed);

        serverAccessTokens.RemoveRange(1, 2);
        serverAccessTokens.ForEach(x => x.AccessToken = "######");

        // Remove all associations
        await IDbContext.PlexAccountServers.ExecuteDeleteAsync();

        // Act
        var request = new AddOrUpdatePlexAccountServersCommand(plexAccount.Id, serverAccessTokens);
        var handler = new AddOrUpdatePlexAccountServersCommandHandler(Log, IDbContext);
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var plexAccountServers = IDbContext
            .PlexAccountServers.Include(x => x.PlexServer)
            .Include(x => x.PlexAccount)
            .ToList();
        plexAccountServers.Count.ShouldBe(serverAccessTokens.Count);

        foreach (var serverAccessToken in serverAccessTokens)
            plexAccountServers
                .Any(x =>
                    x.PlexServer.MachineIdentifier == serverAccessToken.MachineIdentifier
                    && x.AuthToken == "######"
                    && x.PlexAccountId == plexAccount.Id
                )
                .ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldNotAddPlexAccountServerAssociations_WhenAuthTokenIsEmpty()
    {
        // Arrange
        Seed = 45832543;
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 5;
            config.PlexAccountCount = 1;
        });

        var plexAccount = IDbContext.PlexAccounts.FirstOrDefault();
        plexAccount.ShouldNotBeNull();
        var plexServers = IDbContext.PlexServers.ToList();
        var serverAccessTokens = FakeData.GetServerAccessTokenDTO(plexAccount, plexServers, Seed);

        serverAccessTokens[0].AccessToken = string.Empty;
        serverAccessTokens[1].AccessToken = string.Empty;

        // Remove all associations
        await IDbContext.PlexAccountServers.ExecuteDeleteAsync();

        // Act
        var request = new AddOrUpdatePlexAccountServersCommand(plexAccount.Id, serverAccessTokens);
        var handler = new AddOrUpdatePlexAccountServersCommandHandler(Log, IDbContext);
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var plexAccountServers = IDbContext.PlexAccountServers.Include(x => x.PlexServer).ToList();
        plexAccountServers.Count.ShouldBe(3);

        foreach (var serverAccessToken in serverAccessTokens)
            plexAccountServers
                .Any(x =>
                    x.PlexServer.MachineIdentifier == serverAccessToken.MachineIdentifier
                    && x.PlexAccountId == plexAccount.Id
                )
                .ShouldBeTrue();
    }
}
