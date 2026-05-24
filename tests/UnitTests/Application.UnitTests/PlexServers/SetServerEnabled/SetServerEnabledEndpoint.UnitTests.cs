namespace Reaparr.Application.UnitTests;

public class SetServerEnabledEndpointUnitTests : BaseUnitTest<SetServerEnabledEndpoint>
{
    [Test]
    public async Task ShouldEnableServer_WhenServerIsDisabled()
    {
        // Arrange
        await SetupDatabase(91201, config => config.PlexServerCount = 1);

        var dbContext = IDbContext;
        var server = await dbContext.PlexServers.IgnoreIsEnabledFilter().FirstAsync(CancellationToken);

        await dbContext.PlexServers.IgnoreIsEnabledFilter()
            .Where(x => x.Id == server.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsEnabled, false), CancellationToken);

        // Act
        var endpoint = SetupEndpointUnitTest<SetServerEnabledEndpoint>();
        await endpoint.HandleAsync(
            new SetServerEnabledRequest { PlexServerId = server.Id, IsEnabled = true },
            CancellationToken
        );

        // Assert
        endpoint.Response.ShouldNotBeNull();
        endpoint.Response.IsSuccess.ShouldBe(true);

        var updated = await dbContext.PlexServers.IgnoreIsEnabledFilter().FirstAsync(x => x.Id == server.Id, CancellationToken);
        updated.IsEnabled.ShouldBe(true);
    }

    [Test]
    public async Task ShouldDisableServer_WhenServerIsEnabled()
    {
        // Arrange
        await SetupDatabase(91203, config => config.PlexServerCount = 1);

        var dbContext = IDbContext;
        var server = await dbContext.PlexServers.IgnoreIsEnabledFilter().FirstAsync(CancellationToken);

        // Act
        var endpoint = SetupEndpointUnitTest<SetServerEnabledEndpoint>();
        await endpoint.HandleAsync(
            new SetServerEnabledRequest { PlexServerId = server.Id, IsEnabled = false },
            CancellationToken
        );

        // Assert
        endpoint.Response.ShouldNotBeNull();
        endpoint.Response.IsSuccess.ShouldBe(true);

        var updated = await dbContext.PlexServers.IgnoreIsEnabledFilter().FirstAsync(x => x.Id == server.Id, CancellationToken);
        updated.IsEnabled.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldNotChangeUnrelatedServer_WhenUpdatingTargetServer()
    {
        // Arrange
        await SetupDatabase(91204, config => config.PlexServerCount = 2);

        var dbContext = IDbContext;
        var servers = await dbContext.PlexServers.IgnoreIsEnabledFilter().OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var targetId = servers[0].Id;
        var otherId = servers[1].Id;

        // Act
        var endpoint = SetupEndpointUnitTest<SetServerEnabledEndpoint>();
        await endpoint.HandleAsync(new SetServerEnabledRequest { PlexServerId = targetId, IsEnabled = false }, CancellationToken);

        // Assert
        endpoint.Response.ShouldNotBeNull();
        endpoint.Response.IsSuccess.ShouldBe(true);

        var target = await dbContext.PlexServers.IgnoreIsEnabledFilter().FirstAsync(x => x.Id == targetId, CancellationToken);
        var other = await dbContext.PlexServers.IgnoreIsEnabledFilter().FirstAsync(x => x.Id == otherId, CancellationToken);

        target.IsEnabled.ShouldBeFalse();
        other.IsEnabled.ShouldBe(true);
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenServerDoesNotExist()
    {
        // Arrange
        await SetupDatabase(91202, config => config.PlexServerCount = 0);

        // Act
        var endpoint = SetupEndpointUnitTest<SetServerEnabledEndpoint>();
        await endpoint.HandleAsync(
            new SetServerEnabledRequest { PlexServerId = 9999, IsEnabled = true },
            CancellationToken
        );

        // Assert
        endpoint.Response.ShouldNotBeNull();
        endpoint.Response.IsSuccess.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldKeepOwnedOverride_WhenTogglingIsEnabled()
    {
        // Arrange
        await SetupDatabase(91205, config => config.PlexServerCount = 1);
        var db = IDbContext;
        var server = await db.PlexServers.IgnoreIsEnabledFilter().FirstAsync(CancellationToken);

        await db.PlexServers.IgnoreIsEnabledFilter()
            .Where(x => x.Id == server.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, true), CancellationToken);

        // Act
        var endpoint = SetupEndpointUnitTest<SetServerEnabledEndpoint>();
        await endpoint.HandleAsync(new SetServerEnabledRequest { PlexServerId = server.Id, IsEnabled = false }, CancellationToken);

        // Assert
        var updated = await db.PlexServers.IgnoreIsEnabledFilter().FirstAsync(x => x.Id == server.Id, CancellationToken);
        updated.OwnedOverride.ShouldBe(true);
        updated.IsEnabled.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldKeepDownloadsPausedFlag_WhenTogglingIsEnabled()
    {
        // Arrange
        await SetupDatabase(91206, config => config.PlexServerCount = 1);
        var db = IDbContext;
        var server = await db.PlexServers.IgnoreIsEnabledFilter().FirstAsync(CancellationToken);

        await db.PlexServers.IgnoreIsEnabledFilter()
            .Where(x => x.Id == server.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsDownloadsPausedByUser, true), CancellationToken);

        // Act
        var endpoint = SetupEndpointUnitTest<SetServerEnabledEndpoint>();
        await endpoint.HandleAsync(new SetServerEnabledRequest { PlexServerId = server.Id, IsEnabled = false }, CancellationToken);

        // Assert
        var updated = await db.PlexServers.IgnoreIsEnabledFilter().FirstAsync(x => x.Id == server.Id, CancellationToken);
        updated.IsDownloadsPausedByUser.ShouldBe(true);
    }

    [Test]
    public async Task ShouldBeIdempotent_WhenEnablingAlreadyEnabledServer()
    {
        // Arrange
        await SetupDatabase(91207, config => config.PlexServerCount = 1);
        var db = IDbContext;
        var server = await db.PlexServers.IgnoreIsEnabledFilter().FirstAsync(CancellationToken);

        // Act
        var endpoint = SetupEndpointUnitTest<SetServerEnabledEndpoint>();
        await endpoint.HandleAsync(new SetServerEnabledRequest { PlexServerId = server.Id, IsEnabled = true }, CancellationToken);

        // Assert
        endpoint.Response.IsSuccess.ShouldBe(true);
        var updated = await db.PlexServers.IgnoreIsEnabledFilter().FirstAsync(x => x.Id == server.Id, CancellationToken);
        updated.IsEnabled.ShouldBe(true);
    }

    [Test]
    public async Task ShouldBeIdempotent_WhenDisablingAlreadyDisabledServer()
    {
        // Arrange
        await SetupDatabase(91208, config => config.PlexServerCount = 1);
        var db = IDbContext;
        var server = await db.PlexServers.IgnoreIsEnabledFilter().FirstAsync(CancellationToken);
        await db.PlexServers.IgnoreIsEnabledFilter().Where(x => x.Id == server.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsEnabled, false), CancellationToken);

        // Act
        var endpoint = SetupEndpointUnitTest<SetServerEnabledEndpoint>();
        await endpoint.HandleAsync(new SetServerEnabledRequest { PlexServerId = server.Id, IsEnabled = false }, CancellationToken);

        // Assert
        endpoint.Response.IsSuccess.ShouldBe(true);
        var updated = await db.PlexServers.IgnoreIsEnabledFilter().FirstAsync(x => x.Id == server.Id, CancellationToken);
        updated.IsEnabled.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldToggleEnabledStateTwice_WhenTwoRequestsAreSent()
    {
        // Arrange
        await SetupDatabase(91209, config => config.PlexServerCount = 1);
        var db = IDbContext;
        var server = await db.PlexServers.IgnoreIsEnabledFilter().FirstAsync(CancellationToken);
        var endpoint = SetupEndpointUnitTest<SetServerEnabledEndpoint>();

        // Act
        await endpoint.HandleAsync(new SetServerEnabledRequest { PlexServerId = server.Id, IsEnabled = false }, CancellationToken);
        await endpoint.HandleAsync(new SetServerEnabledRequest { PlexServerId = server.Id, IsEnabled = true }, CancellationToken);

        // Assert
        var updated = await db.PlexServers.IgnoreIsEnabledFilter().FirstAsync(x => x.Id == server.Id, CancellationToken);
        updated.IsEnabled.ShouldBe(true);
    }

    [Test]
    public async Task ShouldReturnSuccessResponse_WhenDisablingTargetServer()
    {
        // Arrange
        await SetupDatabase(91210, config => config.PlexServerCount = 1);
        var db = IDbContext;
        var server = await db.PlexServers.IgnoreIsEnabledFilter().FirstAsync(CancellationToken);

        // Act
        var endpoint = SetupEndpointUnitTest<SetServerEnabledEndpoint>();
        await endpoint.HandleAsync(new SetServerEnabledRequest { PlexServerId = server.Id, IsEnabled = false }, CancellationToken);

        // Assert
        endpoint.Response.ShouldNotBeNull();
        endpoint.Response.IsSuccess.ShouldBe(true);
    }

    [Test]
    public void ShouldRejectInvalidPlexServerId_WhenValidatingRequest()
    {
        // Arrange
        var validator = new SetServerEnabledRequestValidator();

        // Act
        var result = validator.Validate(new SetServerEnabledRequest { PlexServerId = 0, IsEnabled = true });

        // Assert
        result.IsValid.ShouldBeFalse();
    }
}

