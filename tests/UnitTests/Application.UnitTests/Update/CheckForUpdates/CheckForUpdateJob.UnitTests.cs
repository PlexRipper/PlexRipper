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

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckForUpdatesCommand>(), CancellationToken))
            .ReturnsAsync(Result.Ok(AppUpdateCheckResult.NoUpdate()))
            .Verifiable(Times.Once());

        // Act
        await Sut.Execute(context);

        // Assert
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<CheckForUpdatesCommand>(), CancellationToken), Times.Once());
    }
}
