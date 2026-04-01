namespace Reaparr.Application.UnitTests;

public class CreateDownloadTasksCommandHandlerUnitTests : BaseUnitTest<CreateDownloadTasksCommandHandler>
{
    [Test]
    public async Task ShouldGenerateAllDownloadTaskTypes_WhenAllMediaTypesAreGiven()
    {
        // Arrange
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskMoviesCommand>).ReturnsAsync(Result.Ok());
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowsCommand>).ReturnsAsync(Result.Ok());
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>).ReturnsAsync(Result.Ok());
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowEpisodesCommand>)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());
        Mock.PublishEvent(It.IsAny<CheckDownloadQueueEvent>).Returns(Task.CompletedTask);
        Mock.Mock<INotificationHubService>()
            .Setup(x =>
                x.SendRefreshNotificationAsync(It.IsAny<List<RefreshDataType>>(), It.IsAny<CancellationToken>())
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        var downloadMediaDtos = new List<DownloadMediaDTO>
        {
            new()
            {
                Type = PlexMediaType.Movie,
                MediaIds = [1, 2, 3],
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
            new()
            {
                Type = PlexMediaType.TvShow,
                MediaIds = [1, 2, 3],
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
            new()
            {
                Type = PlexMediaType.Season,
                MediaIds = [1, 2, 3],
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
            new()
            {
                Type = PlexMediaType.Episode,
                MediaIds = [1, 2, 3],
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
        };

        // Act
        var request = new CreateDownloadTasksCommand(downloadMediaDtos);
        var handler = Mock.Create<CreateDownloadTasksCommandHandler>();
        var result = await handler.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<GenerateDownloadTaskMoviesCommand>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<GenerateDownloadTaskTvShowsCommand>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<GenerateDownloadTaskTvShowEpisodesCommand>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
        Mock.VerifyNotification(It.IsAny<CheckDownloadQueueEvent>, Times.Once);
    }

    [Test]
    public async Task ShouldOnlyGenerateTvShowAndMoviesAndCallCheckDownloadQueue_WhenOnlyTvShowAndMovieMediaIdsAreGiven()
    {
        // Arrange
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskMoviesCommand>).ReturnsAsync(Result.Ok());
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowsCommand>).ReturnsAsync(Result.Ok());
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>).ReturnsAsync(Result.Ok());
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowEpisodesCommand>)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never());
        Mock.PublishEvent(It.IsAny<CheckDownloadQueueEvent>).Returns(Task.CompletedTask);
        Mock.Mock<INotificationHubService>()
            .Setup(x =>
                x.SendRefreshNotificationAsync(It.IsAny<List<RefreshDataType>>(), It.IsAny<CancellationToken>())
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        var downloadMediaDtos = new List<DownloadMediaDTO>
        {
            new()
            {
                Type = PlexMediaType.TvShow,
                MediaIds = [1, 2, 3],
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
            new()
            {
                Type = PlexMediaType.Movie,
                MediaIds = [1, 2, 3],
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
        };

        // Act
        var request = new CreateDownloadTasksCommand(downloadMediaDtos);
        var handler = Mock.Create<CreateDownloadTasksCommandHandler>();
        var result = await handler.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<GenerateDownloadTaskMoviesCommand>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<GenerateDownloadTaskTvShowsCommand>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>(), It.IsAny<CancellationToken>()),
                Times.Never()
            );
        Mock.VerifyNotification(It.IsAny<CheckDownloadQueueEvent>, Times.Once);
    }

    [Test]
    public async Task ShouldNotCallCheckDownloadQueue_WhenNoMediaIdsAreGiven()
    {
        // Arrange
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskMoviesCommand>).ReturnsAsync(Result.Ok());
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowsCommand>).ReturnsAsync(Result.Ok());
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>).ReturnsAsync(Result.Ok());
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowEpisodesCommand>)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never());
        Mock.PublishEvent(It.IsAny<CheckDownloadQueueEvent>).Returns(Task.CompletedTask);
        Mock.Mock<INotificationHubService>()
            .Setup(x =>
                x.SendRefreshNotificationAsync(It.IsAny<List<RefreshDataType>>(), It.IsAny<CancellationToken>())
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never());

        // Act
        var request = new CreateDownloadTasksCommand([]);
        var handler = Mock.Create<CreateDownloadTasksCommandHandler>();
        var result = await handler.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<GenerateDownloadTaskMoviesCommand>(), It.IsAny<CancellationToken>()),
                Times.Never()
            );
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<GenerateDownloadTaskTvShowsCommand>(), It.IsAny<CancellationToken>()),
                Times.Never()
            );
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>(), It.IsAny<CancellationToken>()),
                Times.Never()
            );
        Mock.VerifyNotification(It.IsAny<CheckDownloadQueueEvent>, Times.Never);
        Mock.Mock<INotificationHubService>()
            .Verify(
                x => x.SendRefreshNotificationAsync(It.IsAny<List<RefreshDataType>>(), It.IsAny<CancellationToken>()),
                Times.Never()
            );
    }
}
