using Data.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Data.UnitTests;

public class ChoosePlexServerConnection_UnitTests : BaseUnitTest
{
    public ChoosePlexServerConnection_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnAFailedResult_WhenThePlexServerIdIsInvalid()
    {
        // Act
        var result = await MockIDbContext.Object.ChoosePlexServerConnection(0);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Has400BadRequestError().ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnAFailedResult_WhenThePlexServerIdCannotBeFound()
    {
        // Act
        await SetupDatabase();
        var result = await IDbContext.ChoosePlexServerConnection(999);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Has404NotFoundError().ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnAFailedResult_WhenThereAreNoPlexConnections()
    {
        // Act
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexServerConnectionPerServerCount = 0;
        });
        var result = await IDbContext.ChoosePlexServerConnection(1);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnThePreferredConnection_WhenAPlexServerHasOne()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexServerConnectionPerServerCount = 5;
        });

        var dbContext = IDbContext;
        var plexServer = await dbContext
            .PlexServers.AsTracking()
            .Include(x => x.PlexServerConnections)
            .FirstOrDefaultAsync();
        plexServer.ShouldNotBeNull();
        var preferredConnection = plexServer.PlexServerConnections[2];
        plexServer.PreferredConnectionId = preferredConnection.Id;

        // Add status to all connections
        await dbContext.SaveChangesAsync();

        // Act
        var result = await IDbContext.ChoosePlexServerConnection(plexServer.Id);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(preferredConnection.Id);
    }

    [Fact]
    public async Task ShouldReturnAFailedResult_WhenThereAreOnlyConnectionsWithoutStatus()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexServerConnectionPerServerCount = 1;
        });

        var plexServer = await IDbContext.PlexServers.Include(x => x.PlexServerConnections).FirstOrDefaultAsync();
        plexServer.ShouldNotBeNull();
        await IDbContext.PlexServerStatuses.ExecuteDeleteAsync();

        // Act
        var result = await IDbContext.ChoosePlexServerConnection(plexServer.Id);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnTheConnectionWithPublicAddress_WhenThereIsAValidPublicAddressConnectionsWithStatus()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexServerConnectionPerServerCount = 0;
        });

        var dbContext = IDbContext;
        var plexServer = await dbContext.PlexServers.FirstOrDefaultAsync();
        plexServer.ShouldNotBeNull();

        var plexServerConnections = FakeData.GetPlexServerConnections().Generate(5);
        plexServerConnections[2] = new PlexServerConnection
        {
            Id = plexServerConnections[2].Id,
            Protocol = plexServerConnections[2].Protocol,
            Port = plexServerConnections[2].Port,
            Address = plexServer.PublicAddress,
            Uri = plexServerConnections[2].Uri,
            Local = plexServerConnections[2].Local,
            Relay = plexServerConnections[2].Relay,
            IPv4 = plexServerConnections[2].IPv4,
            IPv6 = plexServerConnections[2].IPv6,
            PortFix = plexServerConnections[2].PortFix,
            PlexServer = plexServerConnections[2].PlexServer,
            PlexServerId = plexServerConnections[2].PlexServerId,
            PlexServerStatus = plexServerConnections[2].PlexServerStatus,
        };

        foreach (var plexServerConnection in plexServerConnections)
        {
            plexServerConnection.PlexServerId = plexServer.Id;
            var status = FakeData.GetPlexServerStatus().Generate();
            status.PlexServerId = plexServer.Id;
            status.PlexServerConnectionId = plexServerConnection.Id;
            plexServerConnection.PlexServerStatus.Add(status);
        }

        dbContext.PlexServerConnections.AddRange(plexServerConnections);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await IDbContext.ChoosePlexServerConnection(plexServer.Id);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(plexServerConnections[2]);
    }
}
