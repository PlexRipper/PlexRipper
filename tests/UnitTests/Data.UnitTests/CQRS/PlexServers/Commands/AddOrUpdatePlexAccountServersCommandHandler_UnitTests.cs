using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.PlexServers;

namespace Data.UnitTests.PlexServers.Commands;

public class AddOrUpdatePlexAccountServersCommandHandler_UnitTests : BaseUnitTest
{
    public AddOrUpdatePlexAccountServersCommandHandler_UnitTests(ITestOutputHelper output) : base(output, true) { }

    [Fact]
    public async Task ShouldAddPlexAccountServerAssociations_WhenNoneExistsYet()
    {
        // Arrange
        void Config(UnitTestDataConfig config) => config.Seed = 45832543;
        DbContext.PlexAccounts.Add(FakeData.GetPlexAccount(Config).Generate());
        DbContext.PlexServers.AddRange(FakeData.GetPlexServer(Config).Generate(5));
        await DbContext.SaveChangesAsync();
        var plexAccount = DbContext.PlexAccounts.FirstOrDefault();
        var plexServers = DbContext.PlexServers.ToList();
        var serverAccessTokens = FakeData.GetServerAccessTokenDTO(plexAccount, plexServers, Config);

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

        void Config(UnitTestDataConfig config) => config.Seed = 194732;
        DbContext.PlexAccounts.Add(FakeData.GetPlexAccount(Config).Generate());
        DbContext.PlexServers.AddRange(FakeData.GetPlexServer(Config).Generate(5));
        await DbContext.SaveChangesAsync();
        var plexAccount = DbContext.PlexAccounts.FirstOrDefault();
        var plexServers = DbContext.PlexServers.ToList();

        var plexAccountServers = FakeData.GetPlexAccountServer(plexAccount, plexServers, Config);
        var serverAccessTokens = FakeData.GetServerAccessTokenDTO(plexAccount, plexServers, Config);

        DbContext.PlexAccountServers.AddRange(plexAccountServers);
        await DbContext.SaveChangesAsync();
        plexAccount = await DbContext.PlexAccounts.FindAsync(1);
        ResetDbContext();
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