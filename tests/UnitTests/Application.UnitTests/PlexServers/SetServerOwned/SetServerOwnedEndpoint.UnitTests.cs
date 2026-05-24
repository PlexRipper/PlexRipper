namespace Reaparr.Application.UnitTests;

public class SetServerOwnedEndpointUnitTests : BaseUnitTest<SetServerOwnedEndpoint>
{
    [Test]
    public async Task ShouldPersistOwnedOverrideOnPlexServer_WhenRequestIsValid()
    {
        // Arrange
        await SetupDatabase(91103, config => config.PlexServerCount = 1);

        var dbContext = IDbContext;
        var plexServer = await dbContext.PlexServers.IgnoreIsEnabledFilter().FirstAsync(CancellationToken);

        await dbContext.PlexServers.IgnoreIsEnabledFilter()
            .Where(x => x.Id == plexServer.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, (bool?)null), CancellationToken);

        // Act
        var endpoint = SetupEndpointUnitTest<SetServerOwnedEndpoint>();
        await endpoint.HandleAsync(new SetServerOwnedRequest { PlexServerId = plexServer.Id, IsOwned = true }, CancellationToken);

        // Assert
        endpoint.Response.ShouldNotBeNull();
        endpoint.Response.IsSuccess.ShouldBe(true);

        var updatedServer = await dbContext.PlexServers.IgnoreIsEnabledFilter().FirstAsync(x => x.Id == plexServer.Id, CancellationToken);
        updatedServer.OwnedOverride.ShouldBe(true);
    }

    [Test]
    public async Task ShouldSetOwnedOverrideFalse_WhenRequestSetsServerNotOwned()
    {
        // Arrange
        await SetupDatabase(91105, config => config.PlexServerCount = 1);

        var dbContext = IDbContext;
        var plexServer = await dbContext.PlexServers.IgnoreIsEnabledFilter().FirstAsync(CancellationToken);

        await dbContext.PlexServers.IgnoreIsEnabledFilter()
            .Where(x => x.Id == plexServer.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, true), CancellationToken);

        // Act
        var endpoint = SetupEndpointUnitTest<SetServerOwnedEndpoint>();
        await endpoint.HandleAsync(new SetServerOwnedRequest { PlexServerId = plexServer.Id, IsOwned = false }, CancellationToken);

        // Assert
        endpoint.Response.ShouldNotBeNull();
        endpoint.Response.IsSuccess.ShouldBe(true);

        var updatedServer = await dbContext.PlexServers.IgnoreIsEnabledFilter().FirstAsync(x => x.Id == plexServer.Id, CancellationToken);
        updatedServer.OwnedOverride.ShouldBe(false);
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenServerDoesNotExist()
    {
        // Arrange
        await SetupDatabase(91110, config => config.PlexServerCount = 0);

        // Act
        var endpoint = SetupEndpointUnitTest<SetServerOwnedEndpoint>();
        await endpoint.HandleAsync(new SetServerOwnedRequest { PlexServerId = 9999, IsOwned = true }, CancellationToken);

        // Assert
        endpoint.Response.IsSuccess.ShouldBe(false);
    }

    [Test]
    public async Task ShouldNotChangeIsEnabled_WhenChangingOwnedOverride()
    {
        // Arrange
        await SetupDatabase(91111, config => config.PlexServerCount = 1);
        var db = IDbContext;
        var server = await db.PlexServers.IgnoreIsEnabledFilter().FirstAsync(CancellationToken);
        await db.PlexServers.IgnoreIsEnabledFilter().Where(x => x.Id == server.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsEnabled, false), CancellationToken);

        // Act
        var endpoint = SetupEndpointUnitTest<SetServerOwnedEndpoint>();
        await endpoint.HandleAsync(new SetServerOwnedRequest { PlexServerId = server.Id, IsOwned = true }, CancellationToken);

        // Assert
        var updated = await db.PlexServers.IgnoreIsEnabledFilter().FirstAsync(x => x.Id == server.Id, CancellationToken);
        updated.IsEnabled.ShouldBe(false);
        updated.OwnedOverride.ShouldBe(true);
    }

    [Test]
    public async Task ShouldToggleOwnedOverrideTwice_WhenTwoRequestsAreSent()
    {
        // Arrange
        await SetupDatabase(91112, config => config.PlexServerCount = 1);
        var db = IDbContext;
        var server = await db.PlexServers.IgnoreIsEnabledFilter().FirstAsync(CancellationToken);
        var endpoint = SetupEndpointUnitTest<SetServerOwnedEndpoint>();

        // Act
        await endpoint.HandleAsync(new SetServerOwnedRequest { PlexServerId = server.Id, IsOwned = true }, CancellationToken);
        await endpoint.HandleAsync(new SetServerOwnedRequest { PlexServerId = server.Id, IsOwned = false }, CancellationToken);

        // Assert
        var updated = await db.PlexServers.IgnoreIsEnabledFilter().FirstAsync(x => x.Id == server.Id, CancellationToken);
        updated.OwnedOverride.ShouldBe(false);
    }

    [Test]
    public async Task ShouldSetOwnedOverrideTrue_WhenCurrentValueIsNull()
    {
        // Arrange
        await SetupDatabase(91113, config => config.PlexServerCount = 1);
        var db = IDbContext;
        var server = await db.PlexServers.IgnoreIsEnabledFilter().FirstAsync(CancellationToken);
        await db.PlexServers.IgnoreIsEnabledFilter().Where(x => x.Id == server.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, (bool?)null), CancellationToken);

        // Act
        var endpoint = SetupEndpointUnitTest<SetServerOwnedEndpoint>();
        await endpoint.HandleAsync(new SetServerOwnedRequest { PlexServerId = server.Id, IsOwned = true }, CancellationToken);

        // Assert
        var updated = await db.PlexServers.IgnoreIsEnabledFilter().FirstAsync(x => x.Id == server.Id, CancellationToken);
        updated.OwnedOverride.ShouldBe(true);
    }

    [Test]
    public async Task ShouldSetOwnedOverrideFalse_WhenCurrentValueIsNull()
    {
        // Arrange
        await SetupDatabase(91114, config => config.PlexServerCount = 1);
        var db = IDbContext;
        var server = await db.PlexServers.IgnoreIsEnabledFilter().FirstAsync(CancellationToken);
        await db.PlexServers.IgnoreIsEnabledFilter().Where(x => x.Id == server.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, (bool?)null), CancellationToken);

        // Act
        var endpoint = SetupEndpointUnitTest<SetServerOwnedEndpoint>();
        await endpoint.HandleAsync(new SetServerOwnedRequest { PlexServerId = server.Id, IsOwned = false }, CancellationToken);

        // Assert
        var updated = await db.PlexServers.IgnoreIsEnabledFilter().FirstAsync(x => x.Id == server.Id, CancellationToken);
        updated.OwnedOverride.ShouldBe(false);
    }

    [Test]
    public async Task ShouldNotUpdateOtherServersOwnedOverride_WhenOneServerIsUpdated()
    {
        // Arrange
        await SetupDatabase(91115, config => config.PlexServerCount = 2);
        var db = IDbContext;
        var servers = await db.PlexServers.IgnoreIsEnabledFilter().OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var targetId = servers[0].Id;
        var otherId = servers[1].Id;

        // Act
        var endpoint = SetupEndpointUnitTest<SetServerOwnedEndpoint>();
        await endpoint.HandleAsync(new SetServerOwnedRequest { PlexServerId = targetId, IsOwned = true }, CancellationToken);

        // Assert
        var target = await db.PlexServers.IgnoreIsEnabledFilter().FirstAsync(x => x.Id == targetId, CancellationToken);
        var other = await db.PlexServers.IgnoreIsEnabledFilter().FirstAsync(x => x.Id == otherId, CancellationToken);
        target.OwnedOverride.ShouldBe(true);
        other.OwnedOverride.ShouldBe(servers[1].OwnedOverride);
    }

    [Test]
    public void ShouldRejectInvalidServerId_WhenValidatingSetOwnedRequest()
    {
        // Arrange
        var validator = new SetServerOwnedRequestValidator();

        // Act
        var result = validator.Validate(new SetServerOwnedRequest { PlexServerId = 0, IsOwned = true });

        // Assert
        result.IsValid.ShouldBe(false);
    }
}

