using Microsoft.EntityFrameworkCore;
using Reaparr.Data.Contracts;

namespace Reaparr.Data.UnitTests;

public class DbContextExtensionsPlexServerUnitTests : BaseUnitTest
{
    public DbContextExtensionsPlexServerUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
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
        var name = await IDbContext.GetPlexServerNameById(server.Id, CancellationToken);

        // Assert
        name.ShouldBe(server.Name);
    }

    [Fact]
    public async Task ShouldReturnServerNameNotFound_WhenPlexServerDoesNotExist()
    {
        // Arrange
        await SetupDatabase(12002);

        // Act
        var name = await IDbContext.GetPlexServerNameById(9999, CancellationToken);

        // Assert
        name.ShouldBe("Server Name Not Found");
    }

    [Fact]
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
        var machineId = await IDbContext.GetPlexServerMachineIdentifierById(server.Id, CancellationToken);

        // Assert
        machineId.ShouldBe(server.MachineIdentifier);
    }

    [Fact]
    public async Task ShouldReturnEmptyMachineIdentifier_WhenPlexServerDoesNotExist()
    {
        // Arrange
        await SetupDatabase(12004);

        // Act
        var machineId = await IDbContext.GetPlexServerMachineIdentifierById(9999, CancellationToken);

        // Assert
        machineId.ShouldBe(string.Empty);
    }

    [Fact]
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

    [Fact]
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
