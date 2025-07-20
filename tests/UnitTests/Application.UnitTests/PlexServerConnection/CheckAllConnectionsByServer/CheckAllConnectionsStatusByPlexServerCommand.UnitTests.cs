using Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application.UnitTests;

public class CheckAllConnectionsStatusByPlexServerCommandUnitTests
    : BaseUnitTest<CheckAllConnectionsStatusByPlexServerHandler>
{
    public CheckAllConnectionsStatusByPlexServerCommandUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnServerNotEnabled_WhenPlexServerIsDisabled()
    {
        // Arrange
        await SetupDatabase(
            5231,
            config =>
            {
                config.PlexServerCount = 1;
            }
        );
        var plexServer = IDbContext.PlexServers.First();
        await IDbContext.PlexServers.ExecuteUpdateAsync(p => p.SetProperty(x => x.IsEnabled, false));

        // Act
        var request = new CheckAllConnectionsStatusByPlexServerCommand(plexServer.Id);
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnEntityNotFound_WhenPlexServerDoesNotExist()
    {
        // Arrange
        await SetupDatabase(5231);
        var request = new CheckAllConnectionsStatusByPlexServerCommand(999);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldReturnNoConnectionsFound_WhenPlexServerHasNoConnections()
    {
        // Arrange
        await SetupDatabase(
            4321,
            config =>
            {
                config.PlexServerCount = 1;
            }
        );

        var dbContext = IDbContext;
        var plexServer = dbContext.PlexServers.First();
        await dbContext.PlexServerConnections.ExecuteDeleteAsync();

        var request = new CheckAllConnectionsStatusByPlexServerCommand(plexServer.Id);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldPublishServerOnlineStatusChangedNotification_WhenOnlineStatusHasChanged()
    {
        // Arrange
        await SetupDatabase(
            54231,
            config =>
            {
                config.PlexServerCount = 1;
            }
        );

        // Set server to offline
        var dbContext = IDbContext;
        await dbContext.PlexServerStatuses.ExecuteDeleteAsync();

        var connections = dbContext.PlexServerConnections.Where(x => x.PlexServerId == 1).ToList();
        foreach (var connection in connections)
        {
            dbContext.PlexServerStatuses.Add(
                FakeData
                    .GetPlexServerStatus(
                        new Seed(connection.Id),
                        isSuccessful: false,
                        plexServerId: 1,
                        plexServerConnectionId: connection.Id
                    )
                    .Generate()
            );
        }

        await dbContext.SaveChangesAsync();

        mock.Mock<ISignalRService>()
            .Setup(m =>
                m.SendRefreshNotificationAsync(It.IsAny<List<RefreshDataType>>(), It.IsAny<CancellationToken>())
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);

        mock.SetupMediator(It.IsAny<CheckConnectionStatusByIdCommand>)
            .ReturnsAsync(
                (CheckConnectionStatusByIdCommand req, CancellationToken _) =>
                    Result.Ok(
                        FakeData
                            .GetPlexServerStatus(
                                new Seed(23),
                                isSuccessful: true,
                                plexServerId: 1,
                                plexServerConnectionId: req.PlexServerConnectionId
                            )
                            .Generate()
                    )
            )
            .Verifiable(Times.AtLeastOnce);

        mock.PublishMediator(It.IsAny<ServerOnlineStatusChangedNotification>)
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);

        // Act
        var request = new CheckAllConnectionsStatusByPlexServerCommand(1);
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldNotPublishServerOnlineStatusChangedNotification_WhenOnlineStatusHasNotChanged()
    {
        // Arrange
        await SetupDatabase(
            523187,
            config =>
            {
                config.PlexServerCount = 1;
            }
        );

        // Set server to online
        var dbContext = IDbContext;
        await dbContext.PlexServerStatuses.ExecuteDeleteAsync();

        var connections = dbContext.PlexServerConnections.Where(x => x.PlexServerId == 1).ToList();
        foreach (var connection in connections)
        {
            dbContext.PlexServerStatuses.Add(
                FakeData
                    .GetPlexServerStatus(
                        new Seed(connection.Id),
                        isSuccessful: true,
                        plexServerId: 1,
                        plexServerConnectionId: connection.Id
                    )
                    .Generate()
            );
        }

        await dbContext.SaveChangesAsync();

        mock.Mock<ISignalRService>()
            .Setup(m =>
                m.SendRefreshNotificationAsync(It.IsAny<List<RefreshDataType>>(), It.IsAny<CancellationToken>())
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);

        mock.SetupMediator(It.IsAny<CheckConnectionStatusByIdCommand>)
            .ReturnsAsync(
                (CheckConnectionStatusByIdCommand req, CancellationToken _) =>
                    Result.Ok(
                        FakeData
                            .GetPlexServerStatus(
                                new Seed(23),
                                isSuccessful: true,
                                plexServerId: 1,
                                plexServerConnectionId: req.PlexServerConnectionId
                            )
                            .Generate()
                    )
            )
            .Verifiable(Times.AtLeastOnce);

        mock.PublishMediator(It.IsAny<ServerOnlineStatusChangedNotification>)
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);

        // Act
        var request = new CheckAllConnectionsStatusByPlexServerCommand(1);
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }
}
