using DownloadManager.Contracts;
using PlexRipper.DownloadManager;

namespace PlexRipper.Application.UnitTests;

public class CreateDownloadTasksCommandHandler_UnitTests : BaseUnitTest<CreateDownloadTasksCommandHandler>
{
    public CreateDownloadTasksCommandHandler_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldGenerateAllDownloadTaskTypes_WhenAllMediaTypesAreGiven()
    {
        // Arrange
        mock.SetupMediator(It.IsAny<GenerateDownloadTaskMoviesCommand>).ReturnsAsync(Result.Ok());
        mock.SetupMediator(It.IsAny<GenerateDownloadTaskTvShowsCommand>).ReturnsAsync(Result.Ok());
        mock.SetupMediator(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>).ReturnsAsync(Result.Ok());
        mock.SetupMediator(It.IsAny<GenerateDownloadTaskTvShowEpisodesCommand>).ReturnsAsync(Result.Ok());
        mock.PublishMediator(It.IsAny<CheckDownloadQueueNotification>).Returns(Task.CompletedTask);

        var downloadMediaDtos = new List<DownloadMediaDTO>()
        {
            new()
            {
                Type = PlexMediaType.Movie,
                MediaIds = new List<int>() { 1, 2, 3 },
            },
            new()
            {
                Type = PlexMediaType.TvShow,
                MediaIds = new List<int>() { 1, 2, 3 },
            },
            new()
            {
                Type = PlexMediaType.Season,
                MediaIds = new List<int>() { 1, 2, 3 },
            },
            new()
            {
                Type = PlexMediaType.Episode,
                MediaIds = new List<int>() { 1, 2, 3 },
            },
        };

        // Act
        var request = new CreateDownloadTasksCommand(downloadMediaDtos);
        var handler = mock.Create<CreateDownloadTasksCommandHandler>();
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.VerifyMediator(It.IsAny<GenerateDownloadTaskMoviesCommand>, Times.Once);
        mock.VerifyMediator(It.IsAny<GenerateDownloadTaskTvShowsCommand>, Times.Once);
        mock.VerifyMediator(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>, Times.Once);
        mock.VerifyMediator(It.IsAny<GenerateDownloadTaskTvShowEpisodesCommand>, Times.Once);
        mock.VerifyNotification(It.IsAny<CheckDownloadQueueNotification>, Times.Once);
    }

    [Fact]
    public async Task ShouldOnlyGenerateTvShowAndMoviesAndCallCheckDownloadQueue_WhenOnlyTvShowAndMovieMediaIdsAreGiven()
    {
        // Arrange
        mock.SetupMediator(It.IsAny<GenerateDownloadTaskMoviesCommand>).ReturnsAsync(Result.Ok());
        mock.SetupMediator(It.IsAny<GenerateDownloadTaskTvShowsCommand>).ReturnsAsync(Result.Ok());
        mock.SetupMediator(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>).ReturnsAsync(Result.Ok());
        mock.SetupMediator(It.IsAny<GenerateDownloadTaskTvShowEpisodesCommand>).ReturnsAsync(Result.Ok());
        mock.PublishMediator(It.IsAny<CheckDownloadQueueNotification>).Returns(Task.CompletedTask);

        var downloadMediaDtos = new List<DownloadMediaDTO>()
        {
            new()
            {
                Type = PlexMediaType.TvShow,
                MediaIds = new List<int>() { 1, 2, 3 },
            },
            new()
            {
                Type = PlexMediaType.Movie,
                MediaIds = new List<int>() { 1, 2, 3 },
            },
        };

        // Act
        var request = new CreateDownloadTasksCommand(downloadMediaDtos);
        var handler = mock.Create<CreateDownloadTasksCommandHandler>();
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.VerifyMediator(It.IsAny<GenerateDownloadTaskMoviesCommand>, Times.Once);
        mock.VerifyMediator(It.IsAny<GenerateDownloadTaskTvShowsCommand>, Times.Once);
        mock.VerifyMediator(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>, Times.Never);
        mock.VerifyMediator(It.IsAny<GenerateDownloadTaskTvShowEpisodesCommand>, Times.Never);
        mock.VerifyNotification(It.IsAny<CheckDownloadQueueNotification>, Times.Once);
    }

    [Fact]
    public async Task ShouldNotCallCheckDownloadQueue_WhenNoMediaIdsAreGiven()
    {
        // Arrange
        mock.SetupMediator(It.IsAny<GenerateDownloadTaskMoviesCommand>).ReturnsAsync(Result.Ok());
        mock.SetupMediator(It.IsAny<GenerateDownloadTaskTvShowsCommand>).ReturnsAsync(Result.Ok());
        mock.SetupMediator(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>).ReturnsAsync(Result.Ok());
        mock.SetupMediator(It.IsAny<GenerateDownloadTaskTvShowEpisodesCommand>).ReturnsAsync(Result.Ok());
        mock.PublishMediator(It.IsAny<CheckDownloadQueueNotification>).Returns(Task.CompletedTask);

        // Act
        var request = new CreateDownloadTasksCommand(new List<DownloadMediaDTO>());
        var handler = mock.Create<CreateDownloadTasksCommandHandler>();
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.VerifyMediator(It.IsAny<GenerateDownloadTaskMoviesCommand>, Times.Never);
        mock.VerifyMediator(It.IsAny<GenerateDownloadTaskTvShowsCommand>, Times.Never);
        mock.VerifyMediator(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>, Times.Never);
        mock.VerifyMediator(It.IsAny<GenerateDownloadTaskTvShowEpisodesCommand>, Times.Never);
        mock.VerifyNotification(It.IsAny<CheckDownloadQueueNotification>, Times.Never);
    }
}