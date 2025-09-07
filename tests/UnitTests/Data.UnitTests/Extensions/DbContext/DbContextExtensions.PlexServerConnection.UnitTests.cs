using Microsoft.EntityFrameworkCore;
using Reaparr.Data.Contracts;

namespace Reaparr.Data.UnitTests;

public class DbContextExtensionsPlexServerConnectionUnitTests : BaseUnitTest
{
    public DbContextExtensionsPlexServerConnectionUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnAFailedResult_WhenThePlexServerIdIsInvalid()
    {
        // Act
        var result = await MockIDbContext.Object.ChoosePlexServerConnection(0, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Has400BadRequestError().ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnAFailedResult_WhenThePlexServerIdCannotBeFound()
    {
        // Act
        await SetupDatabase(47893);
        var result = await IDbContext.ChoosePlexServerConnection(999, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Has404NotFoundError().ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnAFailedResult_WhenThereAreNoPlexConnections()
    {
        // Act
        await SetupDatabase(
            10437,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexServerConnectionPerServerCount = 0;
            }
        );
        var result = await IDbContext.ChoosePlexServerConnection(1, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnThePreferredConnection_WhenAPlexServerHasOne()
    {
        // Arrange
        await SetupDatabase(
            35881,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexServerConnectionPerServerCount = 5;
            }
        );

        var dbContext = IDbContext;
        var plexServer = await dbContext
            .PlexServers.AsTracking()
            .Include(x => x.PlexServerConnections)
            .FirstOrDefaultAsync(CancellationToken);
        plexServer.ShouldNotBeNull();
        var preferredConnection = plexServer.PlexServerConnections.ElementAt(2);
        plexServer.PreferredConnectionId = preferredConnection.Id;

        // Add status to all connections
        await dbContext.SaveChangesAsync(CancellationToken);

        // Act
        var result = await IDbContext.ChoosePlexServerConnection(plexServer.Id, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(preferredConnection.Id);
    }

    [Fact]
    public async Task ShouldReturnAFailedResult_WhenThereAreOnlyConnectionsWithoutStatus()
    {
        // Arrange
        await SetupDatabase(
            71032,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexServerConnectionPerServerCount = 1;
            }
        );

        var plexServer = await IDbContext
            .PlexServers.Include(x => x.PlexServerConnections)
            .FirstOrDefaultAsync(CancellationToken);
        plexServer.ShouldNotBeNull();
        await IDbContext.PlexServerStatuses.ExecuteDeleteAsync(CancellationToken);

        // Act
        var result = await IDbContext.ChoosePlexServerConnection(plexServer.Id, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnTheConnectionWithPublicAddress_WhenThereIsAValidPublicAddressConnectionsWithStatus()
    {
        // Arrange
        var seed = await SetupDatabase(
            17710,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexServerConnectionPerServerCount = 0;
            }
        );

        var dbContext = IDbContext;
        var plexServer = await dbContext.PlexServers.FirstOrDefaultAsync(CancellationToken);
        plexServer.ShouldNotBeNull();

        var plexServerConnections = FakeData.GetPlexServerConnections(seed).Generate(5);

        // Ensure all connections use HTTP protocol (not HTTPS) and are not local
        // so that the public address connection gets prioritized correctly
        for (var i = 0; i < plexServerConnections.Count; i++)
        {
            plexServerConnections[i] = new PlexServerConnection
            {
                Id = plexServerConnections[i].Id,
                Protocol = "http", // Force HTTP to avoid HTTPS priority
                Port = plexServerConnections[i].Port,
                Address = plexServerConnections[i].Address,
                Url = $"http://{plexServerConnections[i].Address}:{plexServerConnections[i].Port}",
                Local = false, // Ensure not local to avoid local priority
                Relay = plexServerConnections[i].Relay,
                IPv4 = plexServerConnections[i].IPv4,
                IPv6 = plexServerConnections[i].IPv6,
                PlexServer = plexServerConnections[i].PlexServer,
                PlexServerId = plexServerConnections[i].PlexServerId,
                LatestConnectionStatus = plexServerConnections[i].LatestConnectionStatus,
                IsCustom = plexServerConnections[i].IsCustom,
            };
        }

        // Set connection at index 2 to have the public address
        plexServerConnections[2] = new PlexServerConnection
        {
            Id = plexServerConnections[2].Id,
            Protocol = "http", // Force HTTP to avoid HTTPS priority
            Port = plexServerConnections[2].Port,
            Address = plexServer.PublicAddress,
            Url = $"http://{plexServer.PublicAddress}:{plexServerConnections[2].Port}",
            Local = false, // Ensure not local to avoid local priority
            Relay = plexServerConnections[2].Relay,
            IPv4 = plexServerConnections[2].IPv4,
            IPv6 = plexServerConnections[2].IPv6,
            PlexServer = plexServerConnections[2].PlexServer,
            PlexServerId = plexServerConnections[2].PlexServerId,
            LatestConnectionStatus = plexServerConnections[2].LatestConnectionStatus,
            IsCustom = plexServerConnections[2].IsCustom,
        };

        foreach (var plexServerConnection in plexServerConnections)
        {
            plexServerConnection.PlexServerId = plexServer.Id;
            var status = FakeData.GetPlexServerStatus(seed).Generate();
            status.PlexServerId = plexServer.Id;
            status.PlexServerConnectionId = plexServerConnection.Id;
            plexServerConnection.LatestConnectionStatus = status;
        }

        dbContext.PlexServerConnections.AddRange(plexServerConnections);
        await dbContext.SaveChangesAsync(CancellationToken);

        // Act
        var result = await IDbContext.ChoosePlexServerConnection(plexServer.Id, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(plexServerConnections[2]);
    }

    [Fact]
    public async Task ShouldReturnNonMainAccountToken_WhenAvailable()
    {
        // Arrange
        var seed = await SetupDatabase(
            12100,
            cfg =>
            {
                cfg.PlexServerCount = 1;
                cfg.PlexAccountCount = 1;
            }
        );
        var db = IDbContext;

        // Add a second, non-main account with access to same server
        var nonMain = FakeData.GetPlexAccount(seed).Generate();
        UpdateInitProperty(nonMain, nameof(PlexAccount.IsMain), false);
        db.PlexAccounts.Add(nonMain);
        await db.SaveChangesAsync(CancellationToken);

        var server = db.PlexServers.First();
        var nonMainAccess = new PlexAccountServer
        {
            PlexAccountId = nonMain.Id,
            PlexServerId = server.Id,
            AuthToken = "NON_MAIN_TOKEN",
            AuthTokenCreationDate = DateTime.UtcNow,
            IsServerOwned = false,
        };
        db.PlexAccountServers.Add(nonMainAccess);
        await db.SaveChangesAsync(CancellationToken);

        // Act
        var result = await db.GetPlexServerTokenAsync(server.Id, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("NON_MAIN_TOKEN");
    }

    [Fact]
    public async Task ShouldFallbackToMainAccountToken_WhenNonMainIsUnavailable()
    {
        // Arrange
        await SetupDatabase(
            12101,
            cfg =>
            {
                cfg.PlexServerCount = 1;
                cfg.PlexAccountCount = 1;
            }
        );
        var db = IDbContext;
        var server = db.PlexServers.First();
        var mainAccess = db.PlexAccountServers.First(x => x.PlexServerId == server.Id);

        // Act
        var result = await db.GetPlexServerTokenAsync(server.Id, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(mainAccess.AuthToken);
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenNoTokensAvailable()
    {
        // Arrange
        await SetupDatabase(
            12102,
            cfg =>
            {
                cfg.PlexServerCount = 1;
                cfg.PlexAccountCount = 0;
            }
        );
        var db = IDbContext;
        var server = db.PlexServers.First();

        // Act
        var result = await db.GetPlexServerTokenAsync(server.Id, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnTokenForSpecificAccount_WhenPlexAccountIdProvided()
    {
        // Arrange
        await SetupDatabase(
            12103,
            cfg =>
            {
                cfg.PlexServerCount = 1;
                cfg.PlexAccountCount = 1;
            }
        );
        var db = IDbContext;
        var access = db.PlexAccountServers.First();

        // Act
        var result = await db.GetPlexServerTokenAsync(access.PlexServerId, access.PlexAccountId, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(access.AuthToken);
    }
}
