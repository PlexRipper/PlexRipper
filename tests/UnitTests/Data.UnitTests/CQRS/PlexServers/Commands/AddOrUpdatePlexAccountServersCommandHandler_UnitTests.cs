using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;

namespace Data.UnitTests.PlexServers.Commands;

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

        // Act
        var handler = new AddOrUpdatePlexAccountServersCommand(Log, IDbContext);
        var result = await handler.ExecuteAsync(plexAccount.Id, serverAccessTokens, CancellationToken.None);

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

        var serverAccessTokens = FakeData.GetServerAccessTokenDTO(plexAccount, plexServers, Seed);

        serverAccessTokens.RemoveRange(1, 2);
        serverAccessTokens.ForEach(x => x.AccessToken = "######");

        // Act
        var handler = new AddOrUpdatePlexAccountServersCommand(Log, IDbContext);
        var result = await handler.ExecuteAsync(plexAccount.Id, serverAccessTokens, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var plexAccountServers2 = IDbContext
            .PlexAccountServers.Include(x => x.PlexServer)
            .Include(x => x.PlexAccount)
            .ToList();
        plexAccountServers2.Count.ShouldBe(serverAccessTokens.Count);

        foreach (var serverAccessToken in serverAccessTokens)
            plexAccountServers2
                .Any(x =>
                    x.PlexServer.MachineIdentifier == serverAccessToken.MachineIdentifier
                    && x.AuthToken == "######"
                    && x.PlexAccountId == plexAccount.Id
                )
                .ShouldBeTrue();
    }
}
