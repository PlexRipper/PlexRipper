using Data.Contracts;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.PlexServers;

namespace Data.UnitTests.PlexServers.Commands;

public class AddOrUpdatePlexAccountServersCommandHandler_UnitTests : BaseUnitTest
{
    public AddOrUpdatePlexAccountServersCommandHandler_UnitTests(ITestOutputHelper output) : base(output) { }

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

        var plexAccount = DbContext.PlexAccounts.FirstOrDefault();
        var plexServers = DbContext.PlexServers.ToList();
        var serverAccessTokens = FakeData.GetServerAccessTokenDTO(plexAccount, plexServers, Seed);

        ResetDbContext();

        // Act
        var request = new AddOrUpdatePlexAccountServersCommand(plexAccount, serverAccessTokens);
        var handler = new AddOrUpdatePlexAccountServersCommandHandler(DbContext);
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        ResetDbContext();
        result.IsSuccess.ShouldBeTrue();
        var plexAccountServers = DbContext
            .PlexAccountServers
            .Include(x => x.PlexServer)
            .ToList();
        plexAccountServers.Count.ShouldBe(serverAccessTokens.Count);

        foreach (var serverAccessToken in serverAccessTokens)
            plexAccountServers
                .Any(x => x.PlexServer.MachineIdentifier == serverAccessToken.MachineIdentifier
                          && x.AuthToken == serverAccessToken.AccessToken
                          && x.PlexAccountId == plexAccount.Id)
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

        var plexAccount = DbContext.PlexAccounts.FirstOrDefault();
        var plexServers = DbContext.PlexServers.ToList();
        ResetDbContext();

        var serverAccessTokens = FakeData.GetServerAccessTokenDTO(plexAccount, plexServers, Seed);

        serverAccessTokens.RemoveRange(1, 2);
        serverAccessTokens.ForEach(x => x.AccessToken = "######");

        // Act
        var request = new AddOrUpdatePlexAccountServersCommand(plexAccount, serverAccessTokens);
        var handler = new AddOrUpdatePlexAccountServersCommandHandler(DbContext);
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        ResetDbContext();
        result.IsSuccess.ShouldBeTrue();
        var plexAccountServers2 = DbContext
            .PlexAccountServers
            .Include(x => x.PlexServer)
            .Include(x => x.PlexAccount)
            .ToList();
        plexAccountServers2.Count.ShouldBe(serverAccessTokens.Count);

        foreach (var serverAccessToken in serverAccessTokens)
            plexAccountServers2
                .Any(x => x.PlexServer.MachineIdentifier == serverAccessToken.MachineIdentifier
                          && x.AuthToken == "######"
                          && x.PlexAccountId == plexAccount.Id)
                .ShouldBeTrue();
    }
}