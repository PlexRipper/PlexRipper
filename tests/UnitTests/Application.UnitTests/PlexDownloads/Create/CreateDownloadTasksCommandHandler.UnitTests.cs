namespace Reaparr.Application.UnitTests;

public class CreateDownloadTasksCommandHandlerUnitTests : BaseUnitTest<CreateDownloadTasksCommandHandler>
{
    private static readonly DownloadTaskCreationReport _moviesReport = new() { Movies = 3 };
    private static readonly DownloadTaskCreationReport _tvShowsReport = new() { TvShows = 2 };
    private static readonly DownloadTaskCreationReport _seasonsReport = new() { Seasons = 4 };
    private static readonly DownloadTaskCreationReport _episodesReport = new() { Episodes = 10 };

    [Test]
    public void CreateDownloadTasksCommandValidator_ShouldRejectNullRequest()
    {
        // Arrange
        var validator = new CreateDownloadTasksCommandValidator();
        var command = new CreateDownloadTasksCommand((CreateDownloadTasksRequest)null!);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(CreateDownloadTasksCommand.Request));
    }

    [Test]
    public async Task ShouldGenerateAllDownloadTaskTypes_WhenAllMediaTypesAreGiven()
    {
        // Arrange
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskMoviesCommand>).ReturnsAsync(Result.Ok(_moviesReport));
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowsCommand>).ReturnsAsync(Result.Ok(_tvShowsReport));
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>).ReturnsAsync(Result.Ok(_seasonsReport));
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowEpisodesCommand>)
            .ReturnsAsync(Result.Ok(_episodesReport))
            .Verifiable(Times.Once());
        Mock.PublishEvent(It.IsAny<CheckDownloadQueueEvent>).Returns(Task.CompletedTask);
        Mock.Mock<INotificationHubService>()
            .Setup(x =>
                x.SendRefreshNotificationAsync(It.IsAny<List<RefreshDataType>>())
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
        result.Value.Movies.ShouldBe(3);
        result.Value.TvShows.ShouldBe(2);
        result.Value.Seasons.ShouldBe(4);
        result.Value.Episodes.ShouldBe(10);
        result.Value.Total.ShouldBe(19);
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
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskMoviesCommand>).ReturnsAsync(Result.Ok(_moviesReport));
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowsCommand>).ReturnsAsync(Result.Ok(_tvShowsReport));
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>).ReturnsAsync(Result.Ok(_seasonsReport));
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowEpisodesCommand>)
            .ReturnsAsync(Result.Ok(_episodesReport))
            .Verifiable(Times.Never());
        Mock.PublishEvent(It.IsAny<CheckDownloadQueueEvent>).Returns(Task.CompletedTask);
        Mock.Mock<INotificationHubService>()
            .Setup(x =>
                x.SendRefreshNotificationAsync(It.IsAny<List<RefreshDataType>>())
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
        result.Value.Movies.ShouldBe(3);
        result.Value.TvShows.ShouldBe(2);
        result.Value.Total.ShouldBe(5);
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
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskMoviesCommand>).ReturnsAsync(Result.Ok(_moviesReport));
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowsCommand>).ReturnsAsync(Result.Ok(_tvShowsReport));
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>).ReturnsAsync(Result.Ok(_seasonsReport));
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowEpisodesCommand>)
            .ReturnsAsync(Result.Ok(_episodesReport))
            .Verifiable(Times.Never());
        Mock.PublishEvent(It.IsAny<CheckDownloadQueueEvent>).Returns(Task.CompletedTask);
        Mock.Mock<INotificationHubService>()
            .Setup(x =>
                x.SendRefreshNotificationAsync(It.IsAny<List<RefreshDataType>>())
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never());

        // Act
        var request = new CreateDownloadTasksCommand([]);
        var handler = Mock.Create<CreateDownloadTasksCommandHandler>();
        var result = await handler.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Total.ShouldBe(0);
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
                x => x.SendRefreshNotificationAsync(It.IsAny<List<RefreshDataType>>()),
                Times.Never()
            );
    }

    [Test]
    public async Task ShouldHaveFailedResult_WhenGeneratingMovieDownloadTasksFails()
    {
        // Arrange
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskMoviesCommand>)
            .ReturnsAsync(Result.Fail("Movie generation failed"))
            .Verifiable(Times.Once());
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowsCommand>).ReturnsAsync(Result.Ok(_tvShowsReport));
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>).ReturnsAsync(Result.Ok(_seasonsReport));
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowEpisodesCommand>).ReturnsAsync(Result.Ok(_episodesReport));
        Mock.PublishEvent(It.IsAny<CheckDownloadQueueEvent>).Returns(Task.CompletedTask);
        Mock.Mock<INotificationHubService>()
            .Setup(x =>
                x.SendRefreshNotificationAsync(It.IsAny<List<RefreshDataType>>())
            )
            .Returns(Task.CompletedTask);

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
        };

        // Act
        var request = new CreateDownloadTasksCommand(downloadMediaDtos);
        var handler = Mock.Create<CreateDownloadTasksCommandHandler>();
        var result = await handler.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(x => x.Message.Contains("Movie generation failed"));
        Mock.VerifyNotification(It.IsAny<CheckDownloadQueueEvent>, Times.Never);
        Mock.Mock<INotificationHubService>()
            .Verify(
                x => x.SendRefreshNotificationAsync(It.IsAny<List<RefreshDataType>>()),
                Times.Never()
            );
    }
}
