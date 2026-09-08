namespace Reaparr.Application.UnitTests;

public class TestConnectionToRadarrEndpointUnitTests
    : BaseEndpointUnitTest<
        TestConnectionToRadarrEndpoint,
        TestConnectionToRadarrEndpointRequest,
        ResultDTO<TestConnectionToRadarrEndpointResponse>
    >
{
    [Test]
    public async Task ShouldReturnHttp200WithUpstreamFailureDetails()
    {
        // Arrange
        var testedAt = new DateTime(2026, 9, 7, 12, 0, 0, DateTimeKind.Utc);
        var request = new TestConnectionToRadarrEndpointRequest { Url = "http://radarr.test", ApiKey = "key" };
        var commandResult = Result.Ok(
            new TestConnectionResult
            {
                Status = TestConnectionStatus.InvalidApiKey,
                HttpStatusCode = StatusCodes.Status401Unauthorized,
                ErrorMessage = "Unauthorized",
                TestedAt = testedAt,
            }
        );

        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(
                    It.Is<TestConnectionToRadarrCommand>(command =>
                        command.IntegrationId == null && command.Url == "http://radarr.test" && command.ApiKey == "key"
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
        result.Value.Result.ShouldBe(TestConnectionStatus.InvalidApiKey);
        result.Value.HttpStatusCode.ShouldBe(StatusCodes.Status401Unauthorized);
        result.Value.ErrorMessage.ShouldBe("Unauthorized");
        result.Value.TestedAt.ShouldBe(testedAt);
        Mock.Mock<ICommandExecutor>().Verify();
    }
}
