using Reaparr.Application.Contracts;
using Reaparr.BaseTests;

namespace Reaparr.Application.UnitTests;

public class CreateDownloadTasksCommandHandler_UnitTests : BaseUnitTest<CreateDownloadTasksCommandHandler>
{
    public CreateDownloadTasksCommandHandler_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldGenerateAllDownloadTaskTypes_WhenAllMediaTypesAreGiven()
    {
        // Arrange
        mock.SetupCommand(It.IsAny<GenerateDownloadTaskMoviesCommand>).ReturnsAsync(Result.Ok());
        mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowsCommand>).ReturnsAsync(Result.Ok());
        mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>).ReturnsAsync(Result.Ok());
        mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowEpisodesCommand>)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);
        mock.PublishEvent(It.IsAny<CheckDownloadQueueEvent>).Returns(Task.CompletedTask);

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
        var handler = mock.Create<CreateDownloadTasksCommandHandler>();
        var result = await handler.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.VerifyEventPublished(It.IsAny<GenerateDownloadTaskMoviesCommand>, Times.Once);
        mock.VerifyEventPublished(It.IsAny<GenerateDownloadTaskTvShowsCommand>, Times.Once);
        mock.VerifyEventPublished(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>, Times.Once);
        mock.VerifyNotification(It.IsAny<CheckDownloadQueueEvent>, Times.Once);
    }

    [Fact]
    public async Task ShouldOnlyGenerateTvShowAndMoviesAndCallCheckDownloadQueue_WhenOnlyTvShowAndMovieMediaIdsAreGiven()
    {
        // Arrange
        mock.SetupCommand(It.IsAny<GenerateDownloadTaskMoviesCommand>).ReturnsAsync(Result.Ok());
        mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowsCommand>).ReturnsAsync(Result.Ok());
        mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>).ReturnsAsync(Result.Ok());
        mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowEpisodesCommand>)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never);
        mock.PublishEvent(It.IsAny<CheckDownloadQueueEvent>).Returns(Task.CompletedTask);

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
        var handler = mock.Create<CreateDownloadTasksCommandHandler>();
        var result = await handler.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.VerifyEventPublished(It.IsAny<GenerateDownloadTaskMoviesCommand>, Times.Once);
        mock.VerifyEventPublished(It.IsAny<GenerateDownloadTaskTvShowsCommand>, Times.Once);
        mock.VerifyEventPublished(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>, Times.Never);
        mock.VerifyNotification(It.IsAny<CheckDownloadQueueEvent>, Times.Once);
    }

    [Fact]
    public async Task ShouldNotCallCheckDownloadQueue_WhenNoMediaIdsAreGiven()
    {
        // Arrange
        mock.SetupCommand(It.IsAny<GenerateDownloadTaskMoviesCommand>).ReturnsAsync(Result.Ok());
        mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowsCommand>).ReturnsAsync(Result.Ok());
        mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>).ReturnsAsync(Result.Ok());
        mock.SetupCommand(It.IsAny<GenerateDownloadTaskTvShowEpisodesCommand>)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never);
        mock.PublishEvent(It.IsAny<CheckDownloadQueueEvent>).Returns(Task.CompletedTask);

        // Act
        var request = new CreateDownloadTasksCommand([]);
        var handler = mock.Create<CreateDownloadTasksCommandHandler>();
        var result = await handler.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.VerifyEventPublished(It.IsAny<GenerateDownloadTaskMoviesCommand>, Times.Never);
        mock.VerifyEventPublished(It.IsAny<GenerateDownloadTaskTvShowsCommand>, Times.Never);
        mock.VerifyEventPublished(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>, Times.Never);
        mock.VerifyNotification(It.IsAny<CheckDownloadQueueEvent>, Times.Never);
    }
}
