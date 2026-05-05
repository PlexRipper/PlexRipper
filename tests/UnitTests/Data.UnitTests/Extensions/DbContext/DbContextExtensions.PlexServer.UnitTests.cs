namespace Reaparr.Data.UnitTests;

public class DbContextExtensionsPlexServerUnitTests : BaseUnitTest
{
    [Test]
    public async Task ShouldReturnServerName_WhenPlexServerExists()
    {
        // Arrange
        await SetupDatabase(
            12001,
            cfg =>
            {
                cfg.PlexServerCount = 1;
            }
        );

        var server = IDbContext.PlexServers.FirstOrDefault();
        server.ShouldNotBeNull();

        // Act
        var name = await IDbContext.GetPlexServerNameById(server.Id);

        // Assert
        name.ShouldBe(server.Name);
    }

    [Test]
    public async Task ShouldReturnServerNameNotFound_WhenPlexServerDoesNotExist()
    {
        // Arrange
        await SetupDatabase(12002);

        // Act
        var name = await IDbContext.GetPlexServerNameById(9999);

        // Assert
        name.ShouldBe("Server Name Not Found");
    }

    [Test]
    public async Task ShouldReturnMachineIdentifier_WhenPlexServerExists()
    {
        // Arrange
        await SetupDatabase(
            12003,
            cfg =>
            {
                cfg.PlexServerCount = 1;
            }
        );

        var server = IDbContext.PlexServers.FirstOrDefault();
        server.ShouldNotBeNull();

        // Act
        var machineId = await IDbContext.GetPlexServerMachineIdentifierById(server.Id);

        // Assert
        machineId.ShouldBe(server.MachineIdentifier);
    }

    [Test]
    public async Task ShouldReturnEmptyMachineIdentifier_WhenPlexServerDoesNotExist()
    {
        // Arrange
        await SetupDatabase(12004);

        // Act
        var machineId = await IDbContext.GetPlexServerMachineIdentifierById(9999);

        // Assert
        machineId.ShouldBe(string.Empty);
    }

    [Test]
    public async Task ShouldReturnTrueForIsServerOnline_WhenThereIsASuccessfulStatus()
    {
        // Arrange
        await SetupDatabase(
            12005,
            cfg =>
            {
                cfg.PlexServerCount = 1;
            }
        );
        var server = IDbContext.PlexServers.FirstOrDefault();
        server.ShouldNotBeNull();

        // Act
        var isOnline = await IDbContext.IsServerOnline(server.Id, CancellationToken);

        // Assert
        isOnline.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturnFalseForIsServerOnline_WhenThereAreNoStatuses()
    {
        // Arrange
        await SetupDatabase(
            12006,
            cfg =>
            {
                cfg.PlexServerCount = 1;
            }
        );
        var server = IDbContext.PlexServers.FirstOrDefault();
        server.ShouldNotBeNull();

        await IDbContext.PlexServerStatuses.ExecuteDeleteAsync(CancellationToken);

        // Act
        var isOnline = await IDbContext.IsServerOnline(server.Id, CancellationToken);

        // Assert
        isOnline.ShouldBeFalse();
    }
}
