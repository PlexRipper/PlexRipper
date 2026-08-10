namespace Reaparr.Application.UnitTests;

public class CheckAllPlexServerConnectionsCommandUnitTests
    : BaseCommandUnitTest<CheckAllPlexServerConnectionsCommand>
{
    [Test]
    public async Task ShouldReturnSuccess_WhenNoPlexServersExist()
    {
        // Arrange
        await SetupDatabase(3150);

        // Act
        var result = await TestHandlerExecuteAsync(new CheckAllPlexServerConnectionsCommand());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.IsAny<CheckAllConnectionsStatusByPlexServerCommand>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never()
            );
    }

    [Test]
    public async Task ShouldCheckEveryEnabledServerAndReturnSuccess_WhenConnectionChecksFail()
    {
        // Arrange
        await SetupDatabase(
            3151,
            config =>
            {
                config.PlexServerCount = 2;
            }
        );
        var plexServerIds = IDbContext.PlexServers.Select(x => x.Id).ToList();

        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(
                    It.IsAny<CheckAllConnectionsStatusByPlexServerCommand>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Fail<List<PlexServerStatus>>("Server is offline"));
        Mock.Mock<IProgressHubService>()
            .Setup(x => x.SendJobStatusUpdateAsync(It.IsAny<JobStatusUpdate<CheckAllConnectionStatusUpdateDTO>>()));

        // Act
        var result = await TestHandlerExecuteAsync(new CheckAllPlexServerConnectionsCommand());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        foreach (var plexServerId in plexServerIds)
        {
            Mock.Mock<ICommandExecutor>()
                .Verify(
                    x =>
                        x.Send(
                            It.Is<CheckAllConnectionsStatusByPlexServerCommand>(command =>
                                command.PlexServerId == plexServerId
                            ),
                            It.IsAny<CancellationToken>()
                        ),
                    Times.Once()
                );
        }

        Mock.Mock<IProgressHubService>()
            .Verify(
                x =>
                    x.SendJobStatusUpdateAsync(
                        It.Is<JobStatusUpdate<CheckAllConnectionStatusUpdateDTO>>(update =>
                            update.Status == JobStatus.Completed
                        )
                    ),
                Times.Once()
            );
    }
}
