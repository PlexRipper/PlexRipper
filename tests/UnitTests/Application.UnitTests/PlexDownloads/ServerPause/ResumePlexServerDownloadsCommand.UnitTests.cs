using Microsoft.EntityFrameworkCore;
using Reaparr.Data.Contracts;

namespace Reaparr.Application.UnitTests;

public class ResumePlexServerDownloadsCommandUnitTests : BaseUnitTest<ResumePlexServerDownloadsCommandHandler>
{
    public ResumePlexServerDownloadsCommandUnitTests()
        : base() { }

    [Test]
    public async Task ShouldResumeServerAndTriggerQueue_WhenServerIsPausedByUser()
    {
        // Arrange
        await SetupDatabase(9811);

        var plexServerId = (await IDbContext.PlexServers.FirstAsync(CancellationToken)).Id;

        await IDbContext
            .PlexServers.Where(x => x.Id == plexServerId)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.IsDownloadsPausedByUser, true), CancellationToken);

        Mock.Mock<IEventPublisher>()
            .Setup(x => x.PublishAsync(It.IsAny<CheckDownloadQueueEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await Sut.ExecuteAsync(new ResumePlexServerDownloadsCommand(plexServerId), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var server = await IDbContext.PlexServers.GetAsync(plexServerId, CancellationToken);
        server.ShouldNotBeNull();
        server!.IsDownloadsPausedByUser.ShouldBeFalse();

        Mock.Mock<IEventPublisher>()
            .Verify(
                x => x.PublishAsync(It.IsAny<CheckDownloadQueueEvent>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
    }
}
