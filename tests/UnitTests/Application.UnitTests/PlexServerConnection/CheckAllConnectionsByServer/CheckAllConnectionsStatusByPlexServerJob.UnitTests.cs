using Quartz;

namespace Reaparr.Application.UnitTests;

public class CheckAllConnectionsStatusByPlexServerJobUnitTests : BaseUnitTest<CheckAllConnectionsStatusByPlexServerJob>
{
    private static Mock<IJobExecutionContext> SetupJobContext()
    {
        var jobDetail = new Mock<IJobDetail>();
        jobDetail.SetupGet(x => x.Key).Returns(CheckAllConnectionsStatusByPlexServerJob.GetJobKey());

        var context = new Mock<IJobExecutionContext>();
        context.SetupGet(x => x.CancellationToken).Returns(CancellationToken.None);
        context.SetupGet(x => x.JobDetail).Returns(jobDetail.Object);
        context.SetupProperty(x => x.Result);
        return context;
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

        var plexServers = await IDbContext
            .PlexServers.Include(x => x.PlexServerConnections)
            .OrderBy(x => x.Id)
            .ToListAsync(CancellationToken);
        var plexServerIds = plexServers.Select(x => x.Id).ToList();
        var expectedPayload = plexServers.ToDictionary(
            x => x.Id,
            x => x.PlexServerConnections.Select(y => y.Id).ToList()
        );

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
        var context = SetupJobContext();

        // Act
        var action = () => Sut.Execute(context.Object);

        // Assert
        await action.ShouldNotThrowAsync();
        var update = context.Object.Result.ShouldBeOfType<JobStatusUpdate<CheckAllConnectionStatusUpdateDTO>>();
        update.Status.ShouldBe(JobStatus.Failed);
        update.Data.PlexServersWithConnectionIds.Count.ShouldBe(expectedPayload.Count);
        update.Data.PlexServersWithConnectionIds.ShouldAllBe(pair =>
            expectedPayload.ContainsKey(pair.Key) && pair.Value.SequenceEqual(expectedPayload[pair.Key])
        );
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<CheckAllConnectionsStatusByPlexServerCommand>(), It.IsAny<CancellationToken>()),
                Times.Exactly(plexServerIds.Count)
            );
    }
}
