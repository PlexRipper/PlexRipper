namespace Reaparr.Application.UnitTests;

public class TestConnectionToSonarrEndpointUnitTests
    : BaseEndpointUnitTest<
        TestConnectionToSonarrEndpoint,
        TestConnectionToSonarrEndpointRequest,
        ResultDTO<TestConnectionToSonarrEndpointResponse>
    >
{
    [Test]
    public async Task ShouldReturnHttp200WithTransportFailureDetails()
    {
        // Arrange
        var testedAt = new DateTime(2026, 9, 7, 12, 0, 0, DateTimeKind.Utc);
        var request = new TestConnectionToSonarrEndpointRequest { Url = "http://sonarr.test", ApiKey = "key" };
        var commandResult = Result.Ok(
            new TestConnectionResult
            {
                Status = TestConnectionStatus.ConnectionFailed,
                HttpStatusCode = null,
                ErrorMessage = "Connection failed.",
                TestedAt = testedAt,
            }
        );

        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(
                    It.Is<TestConnectionToSonarrCommand>(command =>
                        command.IntegrationId == null && command.Url == "http://sonarr.test" && command.ApiKey == "key"
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(commandResult)
            .Verifiable(Times.Once());

        // Act
        var endpointResult = await TestEndpointHandleAsync(request);
        var result = endpointResult.Response!;

        // Assert
        endpointResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Result.ShouldBe(TestConnectionStatus.ConnectionFailed);
        result.Value.HttpStatusCode.ShouldBeNull();
        result.Value.ErrorMessage.ShouldBe("Connection failed.");
        result.Value.TestedAt.ShouldBe(testedAt);
        Mock.Mock<ICommandExecutor>().Verify();
    }
}
