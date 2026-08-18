using System.Collections.Concurrent;
using Quartz;

namespace Reaparr.Application.UnitTests;

public class InspectPlexServerJobUnitTests : BaseUnitTest<InspectPlexServerJob>
{
    private static IJobExecutionContext SetupJobContext(List<int> plexServerIds)
    {
        var jobDetail = new Mock<IJobDetail>();
        jobDetail.SetupGet(x => x.JobDataMap).Returns(new InspectPlexServerJobPayload(plexServerIds).ToJobDataMap());

        var context = new Mock<IJobExecutionContext>();
        context.SetupGet(x => x.JobDetail).Returns(jobDetail.Object);
        context
            .SetupGet(x => x.MergedJobDataMap)
            .Returns(new InspectPlexServerJobPayload(plexServerIds).ToJobDataMap());
        context.SetupGet(x => x.CancellationToken).Returns(CancellationToken.None);
        return context.Object;
    }

    [Test]
    public async Task ShouldInspectServersInParallel_WhenMultipleServersAreQueued()
    {
        // Arrange
        await SetupDatabase(
            90001,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexMovieLibraryCount = 1;
                config.PlexAccountCount = 1;
            }
        );

        var dbContext = IDbContext;
        var plexServerIds = await dbContext
            .PlexServers.OrderBy(x => x.Id)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);
        var expectedLibraryIds = await dbContext
            .PlexLibraries.GroupBy(x => x.PlexServerId)
            .Select(x => x.OrderBy(y => y.Id).Select(y => y.Id).ToList())
            .ToListAsync(CancellationToken);
        var startedServerIds = new ConcurrentDictionary<int, byte>();
        var bothConnectionChecksStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseConnectionChecks = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var context = SetupJobContext(plexServerIds);

        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(
                    It.Is<CheckAllConnectionsStatusByPlexServerCommand>(command => command.Timeout == 5),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns<CheckAllConnectionsStatusByPlexServerCommand, CancellationToken>(
                async (command, cancellationToken) =>
                {
                    startedServerIds.TryAdd(command.PlexServerId, 0);
                    if (startedServerIds.Count == plexServerIds.Count)
                        bothConnectionChecksStarted.SetResult();

                    await releaseConnectionChecks.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
                    return Result.Ok(new List<PlexServerStatus>());
                }
            )
            .Verifiable(Times.Exactly(plexServerIds.Count));

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<RefreshLibraryAccessCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new PlexLibraryAccessRefreshResponse { OfflineServers = [], Reports = [] }))
            .Verifiable(Times.Exactly(plexServerIds.Count));

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<QueueLibrarySyncJobCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Exactly(plexServerIds.Count));

        Mock.Mock<INotificationHubService>()
            .Setup(x =>
                x.SendRefreshNotificationAsync(
                    It.Is<List<RefreshDataType>>(types =>
                        types.SequenceEqual(new[] { RefreshDataType.PlexAccount, RefreshDataType.PlexLibrary })
                    )
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(plexServerIds.Count));

        // Act
        var executeTask = Sut.Execute(context);
        await bothConnectionChecksStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        executeTask.IsCompleted.ShouldBeFalse();
        releaseConnectionChecks.SetResult();
        await executeTask;

        // Assert
        startedServerIds.Keys.OrderBy(x => x).ShouldBe(plexServerIds);
        expectedLibraryIds.ShouldNotBeEmpty();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.Is<CheckAllConnectionsStatusByPlexServerCommand>(command => command.Timeout == 5),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Exactly(plexServerIds.Count)
            );
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<RefreshLibraryAccessCommand>(), It.IsAny<CancellationToken>()),
                Times.Exactly(plexServerIds.Count)
            );
        foreach (var libraryIds in expectedLibraryIds)
        {
            Mock.Mock<ICommandExecutor>()
                .Verify(
                    x =>
                        x.Send(
                            It.Is<QueueLibrarySyncJobCommand>(command =>
                                command.PlexLibraryIds.SequenceEqual(libraryIds)
                            ),
                            It.IsAny<CancellationToken>()
                        ),
                    Times.Once
                );
        }

        Mock.Mock<INotificationHubService>()
            .Verify(
                x =>
                    x.SendRefreshNotificationAsync(
                        It.Is<List<RefreshDataType>>(types =>
                            types.SequenceEqual(new[] { RefreshDataType.PlexAccount, RefreshDataType.PlexLibrary })
                        )
                    ),
                Times.Exactly(plexServerIds.Count)
            );
    }

    [Test]
    public async Task ShouldContinueInspectingRemainingServers_WhenOneServerConnectionCheckFails()
    {
        // Arrange
        await SetupDatabase(
            90002,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexMovieLibraryCount = 1;
                config.PlexAccountCount = 1;
            }
        );

        var dbContext = IDbContext;
        var plexServerIds = await dbContext
            .PlexServers.OrderBy(x => x.Id)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);
        var failedServerId = plexServerIds.First();
        var successfulServerId = plexServerIds.Last();
        var context = SetupJobContext(plexServerIds);

        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(
                    It.Is<CheckAllConnectionsStatusByPlexServerCommand>(command => command.Timeout == 5),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns<CheckAllConnectionsStatusByPlexServerCommand, CancellationToken>(
                (command, _) =>
                {
                    if (command.PlexServerId == failedServerId)
                        return Task.FromResult(Result.Fail<List<PlexServerStatus>>("failed"));

                    return Task.FromResult(Result.Ok(new List<PlexServerStatus>()));
                }
            )
            .Verifiable(Times.Exactly(plexServerIds.Count));

        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(
                    It.Is<RefreshLibraryAccessCommand>(command => command.PlexServerId == successfulServerId),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok(new PlexLibraryAccessRefreshResponse { OfflineServers = [], Reports = [] }))
            .Verifiable(Times.Once);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<QueueLibrarySyncJobCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        Mock.Mock<INotificationHubService>()
            .Setup(x => x.SendRefreshNotificationAsync(It.IsAny<List<RefreshDataType>>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);

        // Act
        await Sut.Execute(context);

        // Assert
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.Is<CheckAllConnectionsStatusByPlexServerCommand>(command => command.Timeout == 5),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Exactly(plexServerIds.Count)
            );
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.Is<RefreshLibraryAccessCommand>(command => command.PlexServerId == successfulServerId),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.Is<RefreshLibraryAccessCommand>(command => command.PlexServerId == failedServerId),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<QueueLibrarySyncJobCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        Mock.Mock<INotificationHubService>()
            .Verify(x => x.SendRefreshNotificationAsync(It.IsAny<List<RefreshDataType>>()), Times.Once);
    }
}
