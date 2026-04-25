using Reaparr.Environment;

namespace Reaparr.Application.UnitTests;

public class CheckForUpdateJobUnitTests : BaseUnitTest<CheckForUpdateJob>
{
    private IJobExecutionContext SetupJobContext()
    {
        Mock.Mock<IJobExecutionContext>().SetupGet(x => x.JobDetail.JobDataMap).Returns(new JobDataMap());
        Mock.Mock<IJobExecutionContext>().SetupGet(x => x.CancellationToken).Returns(CancellationToken);

        return Mock.Create<IJobExecutionContext>();
    }

    [Test]
    public async Task ShouldDispatchCheckForUpdatesCommand_WhenJobExecutes()
    {
        // Arrange
        var context = SetupJobContext();
        var noUpdate = new AppUpdateCheckResult
        {
            IsUpdateAvailable = false,
            NewestVersion = Mock.Create<IAppBuildInfo>().GetInformationalVersion,
            CurrentVersion = Mock.Create<IAppBuildInfo>().GetInformationalVersion,
            ReleaseNotes = [],
        };
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckForUpdatesCommand>(), CancellationToken))
            .ReturnsAsync(Result.Ok(noUpdate))
            .Verifiable(Times.Once());

        // Act
        await Sut.Execute(context);

        // Assert
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<CheckForUpdatesCommand>(), CancellationToken), Times.Once());
    }

    [Test]
    public async Task ShouldSwallowException_WhenCommandExecutorThrows()
    {
        // Arrange
        var context = SetupJobContext();

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckForUpdatesCommand>(), CancellationToken))
            .ThrowsAsync(new InvalidOperationException("Update check failed"))
            .Verifiable(Times.Once());

        // Act
        await Should.NotThrowAsync(() => Sut.Execute(context));

        // Assert
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<CheckForUpdatesCommand>(), CancellationToken), Times.Once());
    }
}
