using Quartz;

namespace Reaparr.Application.UnitTests;

public class RefreshPlexAccountAccessJobUnitTests : BaseUnitTest<RefreshPlexAccountAccessJob>
{
    private static IJobExecutionContext SetupJobContext()
    {
        var context = new Mock<IJobExecutionContext>();
        context.SetupProperty(x => x.Result);
        context.SetupGet(x => x.CancellationToken).Returns(CancellationToken.None);
        return context.Object;
    }

    [Test]
    public async Task ShouldComplete_WhenAccountAccessRefreshSucceeds()
    {
        // Arrange
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<RefreshPlexAccountAccessCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new List<RefreshPlexAccountAccessRapportDTO>()));

        // Act
        var action = () => Sut.Execute(SetupJobContext());

        // Assert
        await action.ShouldNotThrowAsync();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<RefreshPlexAccountAccessCommand>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
    }

    [Test]
    public async Task ShouldComplete_WhenAccountAccessRefreshIsCancelled()
    {
        // Arrange
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<RefreshPlexAccountAccessCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                ResultExtensions
                    .TaskIsCancelled(nameof(RefreshPlexAccountAccessCommand))
                    .ToResult<List<RefreshPlexAccountAccessRapportDTO>>()
            );

        // Act
        var action = () => Sut.Execute(SetupJobContext());

        // Assert
        await action.ShouldNotThrowAsync();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<RefreshPlexAccountAccessCommand>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
    }

    [Test]
    public async Task ShouldFail_WhenAccountAccessRefreshFails()
    {
        // Arrange
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<RefreshPlexAccountAccessCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<List<RefreshPlexAccountAccessRapportDTO>>("Refresh failed"));

        var context = SetupJobContext();

        // Act
        await Sut.Execute(context);

        // Assert
        var result = context.Result.ShouldBeOfType<BackgroundJobResult>();
        result.Status.ShouldBe(JobStatus.Failed);
        result.ErrorSummary.ShouldBe("Refresh failed");
    }
}
