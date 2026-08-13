using TickerQ.Utilities.Base;

namespace Reaparr.Application.UnitTests;

public class RefreshPlexAccountAccessJobUnitTests : BaseUnitTest<RefreshPlexAccountAccessJob>
{
    private static TickerFunctionContext<RefreshPlexAccountAccessJobPayload> SetupJobContext() =>
        new(new TickerFunctionContext(), new RefreshPlexAccountAccessJobPayload());

    [Test]
    public async Task ShouldComplete_WhenAccountAccessRefreshSucceeds()
    {
        // Arrange
        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(
                    It.IsAny<RefreshPlexAccountAccessCommand>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok(new List<RefreshPlexAccountAccessRapportDTO>()));

        // Act
        var action = () => Sut.ExecuteAsync(SetupJobContext(), CancellationToken);

        // Assert
        await action.ShouldNotThrowAsync();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.IsAny<RefreshPlexAccountAccessCommand>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
    }

    [Test]
    public async Task ShouldFail_WhenAccountAccessRefreshFails()
    {
        // Arrange
        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(
                    It.IsAny<RefreshPlexAccountAccessCommand>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Fail<List<RefreshPlexAccountAccessRapportDTO>>("Refresh failed"));

        // Act
        var action = () => Sut.ExecuteAsync(SetupJobContext(), CancellationToken);

        // Assert
        await action.ShouldThrowAsync<InvalidOperationException>();
    }
}
