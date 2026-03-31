namespace Reaparr.Application.UnitTests;

public class StartDownloadTaskEndpointUnitTests : BaseUnitTest
{
    [Test]
    public async Task ShouldReturnSuccessResult_WhenCommandSucceeds()
    {
        // Arrange
        await SetupDatabase(20101);
        var guid = Guid.NewGuid();

        Mock.SetupCommand(It.IsAny<StartDownloadTaskCommand>).ReturnsAsync(Result.Ok());

        // Act
        var ep = SetupEndpointUnitTest<StartDownloadTaskEndpoint>();
        await ep.HandleAsync(new StartDownloadTaskEndpointRequest(guid), CancellationToken);
        var result = ep.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.Is<StartDownloadTaskCommand>(c => c.DownloadTaskGuid == guid),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenCommandFails()
    {
        // Arrange
        await SetupDatabase(20102);
        var guid = Guid.NewGuid();

        Mock.SetupCommand(It.IsAny<StartDownloadTaskCommand>).ReturnsAsync(Result.Fail("Start failed"));

        // Act
        var ep = SetupEndpointUnitTest<StartDownloadTaskEndpoint>();
        await ep.HandleAsync(new StartDownloadTaskEndpointRequest(guid), CancellationToken);
        var result = ep.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.Is<StartDownloadTaskCommand>(c => c.DownloadTaskGuid == guid),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
    }
}
