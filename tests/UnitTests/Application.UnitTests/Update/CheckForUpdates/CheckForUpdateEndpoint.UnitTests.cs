namespace Reaparr.Application.UnitTests;

public class CheckForUpdateEndpointUnitTests : BaseUnitTest
{
    [Test]
    public async Task ShouldReturnSuccessResult_WhenTriggerSucceeds()
    {
        // Arrange
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckForUpdatesCommand>(), CancellationToken))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());

        // Act
        var endpoint = SetupEndpointUnitTest<CheckForUpdateEndpoint>();
        await endpoint.HandleAsync(CancellationToken);
        var result = endpoint.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();

        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<CheckForUpdatesCommand>(), CancellationToken), Times.Once());
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenTriggerFails()
    {
        // Arrange
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckForUpdatesCommand>(), CancellationToken))
            .ReturnsAsync(Result.Fail("Scheduler error"))
            .Verifiable(Times.Once());

        // Act
        var endpoint = SetupEndpointUnitTest<CheckForUpdateEndpoint>();
        await endpoint.HandleAsync(CancellationToken);
        var result = endpoint.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();

        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<CheckForUpdatesCommand>(), CancellationToken), Times.Once());
    }
}
