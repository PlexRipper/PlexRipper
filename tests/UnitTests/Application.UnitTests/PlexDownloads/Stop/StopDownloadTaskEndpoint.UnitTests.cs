using Reaparr.Application.Contracts;

namespace Reaparr.Application.UnitTests;

public class StopDownloadTaskEndpointUnitTests : BaseUnitTest
{
    public StopDownloadTaskEndpointUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnSuccessResult_WhenCommandSucceeds()
    {
        // Arrange
        await SetupDatabase(20201);
        var guid = Guid.NewGuid();

        Mock.SetupCommand(It.IsAny<StopDownloadTaskCommand>).ReturnsAsync(Result.Ok());

        // Act
        var ep = SetupEndpointUnitTest<StopDownloadTaskEndpoint>();
        await ep.HandleAsync(new StopDownloadTaskEndpointRequest(guid), CancellationToken);
        var result = ep.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.Is<StopDownloadTaskCommand>(c => c.DownloadTaskGuid == guid),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenCommandFails()
    {
        // Arrange
        await SetupDatabase(20202);
        var guid = Guid.NewGuid();

        Mock.SetupCommand(It.IsAny<StopDownloadTaskCommand>).ReturnsAsync(Result.Fail("Stop failed"));

        // Act
        var ep = SetupEndpointUnitTest<StopDownloadTaskEndpoint>();
        await ep.HandleAsync(new StopDownloadTaskEndpointRequest(guid), CancellationToken);
        var result = ep.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.Is<StopDownloadTaskCommand>(c => c.DownloadTaskGuid == guid),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
    }
}
