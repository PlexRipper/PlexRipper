using Quartz;

namespace Reaparr.Application.UnitTests;

public class CheckAllConnectionsStatusByPlexServerJobUnitTests : BaseUnitTest<CheckAllConnectionsStatusByPlexServerJob>
{
    private static IJobExecutionContext SetupJobContext()
    {
        var context = new Mock<IJobExecutionContext>();
        context.SetupGet(x => x.CancellationToken).Returns(CancellationToken.None);
        return context.Object;
    }

    [Test]
    public async Task ShouldComplete_WhenOneOrMorePlexServersAreUnavailable()
    {
        // Arrange
        await SetupDatabase(
            135044,
            config =>
            {
                config.PlexServerCount = 2;
            }
        );

        var plexServerIds = await IDbContext
            .PlexServers.OrderBy(x => x.Id)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckAllConnectionsStatusByPlexServerCommand>(), It.IsAny<CancellationToken>()))
            .Returns<CheckAllConnectionsStatusByPlexServerCommand, CancellationToken>(
                (command, _) =>
                    Task.FromResult(
                        command.PlexServerId == plexServerIds[0]
                            ? Result.Fail<List<PlexServerStatus>>("Plex server is unavailable")
                            : Result.Ok(new List<PlexServerStatus>())
                    )
            );
        Mock.Mock<IProgressHubService>()
            .Setup(x => x.SendJobStatusUpdateAsync(It.IsAny<JobStatusUpdate<CheckAllConnectionStatusUpdateDTO>>()))
            .Returns(Task.CompletedTask);

        // Act
        var action = () => Sut.Execute(SetupJobContext());

        // Assert
        await action.ShouldNotThrowAsync();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<CheckAllConnectionsStatusByPlexServerCommand>(), It.IsAny<CancellationToken>()),
                Times.Exactly(plexServerIds.Count)
            );
    }
}
