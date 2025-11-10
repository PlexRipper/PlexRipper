using Reaparr.Application.Contracts;

namespace Reaparr.Application.UnitTests;

public class CreateDownloadTasksCommandHandlerUnitTests : BaseUnitTest<CreateDownloadTasksCommandHandler>
{
    public CreateDownloadTasksCommandHandlerUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldGenerateAllDownloadTaskTypes_WhenAllMediaTypesAreGiven()
    {
        // Arrange
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskMoviesCommand>).ReturnsAsync(Result.Ok());
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowsCommand>).ReturnsAsync(Result.Ok());
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>).ReturnsAsync(Result.Ok());
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowEpisodesCommand>)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);
        Mock.PublishEvent(It.IsAny<CheckDownloadQueueEvent>).Returns(Task.CompletedTask);

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
        Mock.VerifyEventPublished(It.IsAny<GenerateDownloadTaskMoviesCommand>, Times.Once);
        Mock.VerifyEventPublished(It.IsAny<GenerateDownloadTaskTvShowsCommand>, Times.Once);
        Mock.VerifyEventPublished(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>, Times.Once);
        Mock.VerifyNotification(It.IsAny<CheckDownloadQueueEvent>, Times.Once);
    }

    [Fact]
    public async Task ShouldOnlyGenerateTvShowAndMoviesAndCallCheckDownloadQueue_WhenOnlyTvShowAndMovieMediaIdsAreGiven()
    {
        // Arrange
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskMoviesCommand>).ReturnsAsync(Result.Ok());
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowsCommand>).ReturnsAsync(Result.Ok());
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>).ReturnsAsync(Result.Ok());
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowEpisodesCommand>)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never);
        Mock.PublishEvent(It.IsAny<CheckDownloadQueueEvent>).Returns(Task.CompletedTask);

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
        var request = new CreateDownloadTasksCommand(new CreateDownloadTasksRequest(downloadMediaDtos));
        var handler = Mock.Create<CreateDownloadTasksCommandHandler>();
        var result = await handler.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.VerifyEventPublished(It.IsAny<GenerateDownloadTaskMoviesCommand>, Times.Once);
        Mock.VerifyEventPublished(It.IsAny<GenerateDownloadTaskTvShowsCommand>, Times.Once);
        Mock.VerifyEventPublished(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>, Times.Never);
        Mock.VerifyNotification(It.IsAny<CheckDownloadQueueEvent>, Times.Once);
    }

    [Fact]
    public async Task ShouldNotCallCheckDownloadQueue_WhenNoMediaIdsAreGiven()
    {
        // Arrange
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskMoviesCommand>).ReturnsAsync(Result.Ok());
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowsCommand>).ReturnsAsync(Result.Ok());
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>).ReturnsAsync(Result.Ok());
        Mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowEpisodesCommand>)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never);
        Mock.PublishEvent(It.IsAny<CheckDownloadQueueEvent>).Returns(Task.CompletedTask);

        // Act
        var request = new CreateDownloadTasksCommand([]);
        var handler = Mock.Create<CreateDownloadTasksCommandHandler>();
        var result = await handler.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.VerifyEventPublished(It.IsAny<GenerateDownloadTaskMoviesCommand>, Times.Never);
        Mock.VerifyEventPublished(It.IsAny<GenerateDownloadTaskTvShowsCommand>, Times.Never);
        Mock.VerifyEventPublished(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>, Times.Never);
        Mock.VerifyNotification(It.IsAny<CheckDownloadQueueEvent>, Times.Never);
    }
}
