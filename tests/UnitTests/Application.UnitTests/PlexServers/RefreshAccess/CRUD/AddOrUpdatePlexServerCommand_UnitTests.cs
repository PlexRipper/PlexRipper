namespace PlexRipper.Application.UnitTests;

public class AddOrUpdatePlexServerCommand_UnitTests : BaseUnitTest
{
    public AddOrUpdatePlexServerCommand_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldAddAllServers_WhenNoneExistInTheDatabase()
    {
        // Arrange
        Seed = 45832543;
        await SetupDatabase();
        var expectedPlexServers = FakeData.GetPlexServer(Seed).Generate(5);

        // Act
        var request = new AddOrUpdatePlexServersCommand(expectedPlexServers);
        var handler = new AddOrUpdatePlexServersCommandHandler(Log, IDbContext);
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var plexServersDbs = IDbContext.PlexServers.Include(x => x.PlexServerConnections).ToList();
        plexServersDbs.Count.ShouldBe(5);

        foreach (var expectedPlexServer in expectedPlexServers)
        {
            var plexServerDb = plexServersDbs.Find(x => x.MachineIdentifier == expectedPlexServer.MachineIdentifier);
            plexServerDb.ShouldNotBeNull();
            plexServerDb.ShouldBe(expectedPlexServer);
            plexServerDb.PlexServerConnections.Count.ShouldBe(expectedPlexServer.PlexServerConnections.Count);
            plexServerDb.PlexServerConnections.ShouldBe(expectedPlexServer.PlexServerConnections, true);
        }
    }

    [Fact]
    public async Task ShouldKeepTheSameServerConnectionIds_WhenOnlyTheConnectionPropertiesHaveChanged()
    {
        // Arrange
        Seed = 23724;
        await SetupDatabase(config => config.PlexServerCount = 5);
        var plexServers = IDbContext.PlexServers.Include(x => x.PlexServerConnections).ToList();
        plexServers.Count.ShouldBe(5);

        // Update data setup
        var updatedServers = plexServers.Take(2).ToList();

        // Simulate the server being unchanged, and only the connections having changed except the connection
        // address which it is matched on during updating
        foreach (var updatedServer in updatedServers)
        {
            var connectionCount = updatedServer.PlexServerConnections.Count;
            var updatedConnections = FakeData.GetPlexServerConnections().Generate(connectionCount);
            for (var i = 0; i < connectionCount; i++)
                updatedConnections[i].Address = updatedServer.PlexServerConnections[i].Address;

            updatedServer.PlexServerConnections = updatedConnections;
        }

        // Act
        // Now update
        var request = new AddOrUpdatePlexServersCommand(updatedServers);
        var handler = new AddOrUpdatePlexServersCommandHandler(Log, IDbContext);
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var plexServersDbs = IDbContext.PlexServers.Include(x => x.PlexServerConnections).ToList();
        plexServersDbs.Count.ShouldBe(5);

        foreach (var expectedServer in updatedServers)
        {
            var plexServerDb = plexServersDbs.Find(x => x.MachineIdentifier == expectedServer.MachineIdentifier);
            plexServerDb.ShouldNotBeNull();

            foreach (var plexServerConnectionDb in plexServerDb.PlexServerConnections)
            {
                var expectedConnection = expectedServer.PlexServerConnections.Find(x =>
                    x.Address == plexServerConnectionDb.Address
                );
                expectedConnection.ShouldNotBeNull();
                plexServerConnectionDb.Id.ShouldBe(expectedConnection.Id);
                plexServerConnectionDb.ShouldBe(expectedConnection);
            }
        }
    }

    [Fact]
    public async Task ShouldUpdateSomeAndSyncServersWithConnections_WhenSomeServerConnectionsHaveChangedAndSomeExistInTheDatabase()
    {
        // Arrange
        Seed = 23724;
        await SetupDatabase(config => config.PlexServerCount = 5);
        var plexServers = IDbContext.PlexServers.Include(x => x.PlexServerConnections).ToList();
        var changedPlexServers = FakeData.GetPlexServer(9236).Generate(3);

        var expectedPlexServers = new List<PlexServer>()
        {
            changedPlexServers[0],
            changedPlexServers[1],
            changedPlexServers[2],
            plexServers[3],
            plexServers[4],
        };

        // Create updated servers with the same machineId
        for (var i = 0; i < changedPlexServers.Count; i++)
            changedPlexServers[i].MachineIdentifier = plexServers[i].MachineIdentifier;

        // Act
        // First add the 5 servers
        var request = new AddOrUpdatePlexServersCommand(plexServers);
        var handler = new AddOrUpdatePlexServersCommandHandler(Log, IDbContext);
        var addResult = await handler.Handle(request, CancellationToken.None);

        // Now update
        request = new AddOrUpdatePlexServersCommand(changedPlexServers);
        handler = new AddOrUpdatePlexServersCommandHandler(Log, IDbContext);
        var updateResult = await handler.Handle(request, CancellationToken.None);

        // Assert
        addResult.IsSuccess.ShouldBeTrue();
        updateResult.IsSuccess.ShouldBeTrue();
        var plexServersDbs = IDbContext.PlexServers.Include(x => x.PlexServerConnections).ToList();
        plexServersDbs.Count.ShouldBe(5);

        foreach (var expectedPlexServer in expectedPlexServers)
        {
            var plexServerDb = plexServersDbs.Find(x => x.MachineIdentifier == expectedPlexServer.MachineIdentifier);
            plexServerDb.ShouldNotBeNull();
            plexServerDb.ShouldBe(expectedPlexServer);
            plexServerDb.PlexServerConnections.Count.ShouldBe(expectedPlexServer.PlexServerConnections.Count);
            plexServerDb.PlexServerConnections.ShouldBe(expectedPlexServer.PlexServerConnections, true);
        }
    }

    [Fact]
    public async Task ShouldSyncConnectionsAndKeepTheSameServerConnectionIds_WhenSomeHaveConnectionHaveChanged()
    {
        // Arrange
        Seed = 23724;
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexServerConnectionPerServerCount = 5;
        });

        var plexServer = IDbContext.PlexServers.Include(x => x.PlexServerConnections).FirstOrDefault();
        plexServer.ShouldNotBeNull();
        plexServer.PlexServerConnections.Count.ShouldBe(5);

        // Update data setup
        plexServer.PlexServerConnections.RemoveAt(0);
        plexServer.PlexServerConnections.RemoveAt(0);
        plexServer.PlexServerConnections.RemoveAt(0);
        plexServer.PlexServerConnections.RemoveAt(0);

        var newConnections = FakeData.GetPlexServerConnections().Generate(5);
        plexServer.PlexServerConnections.AddRange(newConnections);

        // Act
        // Now update
        var request = new AddOrUpdatePlexServersCommand([plexServer]);
        var handler = new AddOrUpdatePlexServersCommandHandler(Log, IDbContext);
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var plexServersDb = IDbContext.PlexServers.Include(x => x.PlexServerConnections).FirstOrDefault();
        plexServersDb.ShouldNotBeNull();
        plexServersDb.PlexServerConnections.Count.ShouldBe(6);

        foreach (var plexServerConnection in plexServersDb.PlexServerConnections)
        {
            var expectedConnection = plexServer.PlexServerConnections.Find(x => x.Equals(plexServerConnection));
            expectedConnection.ShouldNotBeNull();
            plexServerConnection.Id.ShouldBe(expectedConnection.Id);
            plexServerConnection.ShouldBe(expectedConnection);
        }
    }
}
