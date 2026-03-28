using System.Linq;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application.UnitTests;

public class RestartDownloadTaskCommandUnitTests : BaseUnitTest<RestartDownloadTaskCommandHandler>
{
    public RestartDownloadTaskCommandUnitTests()
        : base() { }

    [Test]
    public async Task ShouldRequeueDownloadTasks_WhenRestartingValidId()
    {
        // Arrange
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<Reaparr.Domain.DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok());
        await SetupDatabase(72153, config => config.MovieDownloadTasksCount = 1);

        var downloadTasks = await IDbContext.GetAllDownloadTasksByServerAsync(cancellationToken: CancellationToken);
        var movieTask = downloadTasks.First();
        var childKeys = await IDbContext.GetDownloadableChildTaskKeys(movieTask.ToKey(), CancellationToken);

        childKeys.Count.ShouldBeGreaterThan(0);

        foreach (var childKey in childKeys)
        {
            await IDbContext
                .DownloadTaskMovieFile.Where(x => x.Id == childKey.Id)
                .ExecuteUpdateAsync(
                    p => p.SetProperty(x => x.DownloadStatus, DownloadStatus.Stopped),
                    CancellationToken
                );
        }

        Mock.SetupCommand(It.IsAny<StopDownloadTaskCommand>).ReturnsAsync(Result.Ok());
        Mock.PublishEvent(It.IsAny<CheckDownloadQueueEvent>).Returns(Task.CompletedTask);

        // Act
        var result = await Sut.ExecuteAsync(new RestartDownloadTaskCommand(movieTask.Id), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        foreach (var childKey in childKeys)
        {
            var task = await IDbContext.GetDownloadTaskFileAsync(childKey, CancellationToken);
            task.ShouldNotBeNull();
            task!.DownloadStatus.ShouldBe(DownloadStatus.Stopped);

            Mock.VerifyEventPublished(() => new StopDownloadTaskCommand(childKey.Id), Times.Once());
            Mock.Mock<IDownloadTaskUpdateDispatcher>()
                .Verify(
                    x =>
                        x.OnStatusChangedAsync(
                            It.Is<DownloadTaskKey>(k => k == childKey),
                            DownloadStatus.Restarting,
                            It.IsAny<CancellationToken>()
                        ),
                    Times.Once()
                );
            Mock.Mock<IDownloadTaskUpdateDispatcher>()
                .Verify(
                    x =>
                        x.OnStatusChangedAsync(
                            It.Is<DownloadTaskKey>(k => k == childKey),
                            DownloadStatus.Queued,
                            It.IsAny<CancellationToken>()
                        ),
                    Times.Once()
                );
        }

        Mock.Mock<IEventPublisher>()
            .Verify(
                x =>
                    x.PublishAsync(
                        It.Is<CheckDownloadQueueEvent>(e =>
                            e.PlexServerIds.SequenceEqual(new[] { movieTask.PlexServerId })
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
    }
}
