namespace Reaparr.Application.UnitTests;

public class ResumePlexServerDownloadsCommandUnitTests : BaseUnitTest<ResumePlexServerDownloadsCommandHandler>
{
    [Test]
    public async Task ShouldResumeServerAndTriggerQueue_WhenServerIsPausedByUser()
    {
        // Arrange
        await SetupDatabase(9811);

        var dbContext = IDbContext;
        var plexServerId = (await dbContext.PlexServers.FirstAsync(CancellationToken)).Id;

        await dbContext
            .PlexServers.Where(x => x.Id == plexServerId)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.IsDownloadsPausedByUser, true), CancellationToken);

        Mock.Mock<IEventPublisher>()
            .Setup(x => x.PublishAsync(It.IsAny<CheckDownloadQueueEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(new ResumePlexServerDownloadsCommand(plexServerId), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var server = await dbContext.PlexServers.IgnoreIsEnabledFilter().GetAsync(plexServerId, CancellationToken);
        server.ShouldNotBeNull();
        server.IsDownloadsPausedByUser.ShouldBeFalse();

        Mock.Mock<IEventPublisher>().Verify();
    }

    [Test]
    public async Task ShouldReturnFailedResultAndNotPublishEvent_WhenServerIsDisabled()
    {
        // Arrange
        await SetupDatabase(9812);

        var dbContext = IDbContext;
        var plexServerId = (await dbContext.PlexServers.IgnoreIsEnabledFilter().FirstAsync(CancellationToken)).Id;

        await dbContext
            .PlexServers.IgnoreIsEnabledFilter()
            .Where(x => x.Id == plexServerId)
            .ExecuteUpdateAsync(
                p => p.SetProperty(x => x.IsEnabled, false).SetProperty(x => x.IsDownloadsPausedByUser, true),
                CancellationToken
            );

        Mock.Mock<IEventPublisher>()
            .Setup(x => x.PublishAsync(It.IsAny<CheckDownloadQueueEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never());

        // Act
        var result = await Sut.ExecuteAsync(new ResumePlexServerDownloadsCommand(plexServerId), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();

        var server = await dbContext.PlexServers.IgnoreIsEnabledFilter().GetAsync(plexServerId, CancellationToken);
        server.ShouldNotBeNull();
        server.IsDownloadsPausedByUser.ShouldBeTrue();

        Mock.Mock<IEventPublisher>().Verify();
    }
}

