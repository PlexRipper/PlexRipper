using Quartz;

namespace Reaparr.Application.UnitTests;

public class CheckForUpdateJobUnitTests : BaseUnitTest<CheckForUpdateJob>
{
    private const string CURRENT_VERSION = "1.2.3";

    private static IJobExecutionContext SetupJobContext(CancellationToken cancellationToken)
    {
        var context = new Mock<IJobExecutionContext>();
        context.SetupProperty(x => x.Result);
        context.SetupGet(x => x.CancellationToken).Returns(cancellationToken);
        return context.Object;
    }

    [Test]
    public async Task ShouldDispatchCheckForUpdatesCommand_WhenJobExecutes()
    {
        // Arrange
        SetAppBuildInfo(x => x.InformationalVersion = CURRENT_VERSION);
        var noUpdate = new AppUpdateCheckResult
        {
            IsUpdateAvailable = false,
            NewestVersion = CURRENT_VERSION,
            CurrentVersion = CURRENT_VERSION,
            ReleaseNotes = [],
        };
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckForUpdatesCommand>(), CancellationToken))
            .ReturnsAsync(Result.Ok(noUpdate))
            .Verifiable(Times.Once());
        var context = SetupJobContext(CancellationToken);

        // Act
        await Sut.Execute(context);

        // Assert
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<CheckForUpdatesCommand>(), CancellationToken), Times.Once());
    }

    [Test]
    public async Task ShouldSetFailedResult_WhenCommandExecutorThrows()
    {
        // Arrange
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckForUpdatesCommand>(), CancellationToken))
            .ThrowsAsync(new InvalidOperationException("Update check failed"))
            .Verifiable(Times.Once());
        var context = SetupJobContext(CancellationToken);

        // Act
        await Sut.Execute(context);

        // Assert
        var result = context.Result.ShouldBeOfType<BackgroundJobResult>();
        result.Status.ShouldBe(JobStatus.Failed);
        result.ErrorSummary.ShouldBe("Update check failed");
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<CheckForUpdatesCommand>(), CancellationToken), Times.Once());
    }
}
