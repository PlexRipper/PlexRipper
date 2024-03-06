using Data.Contracts;
using DownloadManager.Contracts;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application.UnitTests;

public class CreateDownloadTasksCommandHandler_UnitTests : BaseUnitTest<CreateDownloadTasksCommandHandler>
{
    public CreateDownloadTasksCommandHandler_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldCreateAllDownloadTasks_WhenAllAreNew()
    {
        // Arrange
        await SetupDatabase(config => config.DisableForeignKeyCheck = true);
        var downloadTasks = FakeData.GetDownloadTaskTvShow(options: config =>
            {
                config.MovieDownloadTasksCount = 5;
                config.TvShowDownloadTasksCount = 5;
                config.TvShowSeasonDownloadTasksCount = 5;
                config.TvShowEpisodeDownloadTasksCount = 5;
            })
            .Generate(1);

        mock.Mock<IMediator>().Setup(x => x.Publish(It.IsAny<CheckDownloadQueue>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var request = new CreateDownloadTasksCommand(null);
        var handler = mock.Create<CreateDownloadTasksCommandHandler>();
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var downloadTaskMovies = await DbContext.DownloadTaskMovie.IncludeAll().ToListAsync();
        var downloadTaskTvShows = await DbContext.DownloadTaskTvShow.IncludeAll().ToListAsync();
        var flattenDownloadTasks = downloadTaskMovies.Select(x => x.ToGeneric())
            .Flatten(x => x.Children)
            .ToList()
            .Concat(downloadTaskTvShows.Select(x => x.ToGeneric()).Flatten(x => x.Children))
            .ToList();

        flattenDownloadTasks.Count().ShouldBe(flattenDownloadTasks.Count);
    }

    [Fact]
    public async Task ShouldCreateOnlyChildDownloadTasks_WhenParentAlreadyExists()
    {
        // Arrange
        await SetupDatabase(config => config.DisableForeignKeyCheck = true);
        var downloadTasks = FakeData.GetDownloadTaskTvShow().Generate(1);
        await DbContext.DownloadTaskTvShow.AddAsync(downloadTasks.First());
        await DbContext.SaveChangesAsync();

        mock.Mock<IMediator>().Setup(x => x.Publish(It.IsAny<CheckDownloadQueue>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var request = new CreateDownloadTasksCommand(null);
        var handler = mock.Create<CreateDownloadTasksCommandHandler>();
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var downloadTaskMovies = await DbContext.DownloadTaskMovie.IncludeAll().ToListAsync();
        var downloadTaskTvShows = await DbContext.DownloadTaskTvShow.IncludeAll().ToListAsync();
        var flattenDownloadTasks = downloadTaskMovies.Select(x => x.ToGeneric())
            .Flatten(x => x.Children)
            .ToList()
            .Concat(downloadTaskTvShows.Select(x => x.ToGeneric()).Flatten(x => x.Children))
            .ToList();

        flattenDownloadTasks.Count().ShouldBe(flattenDownloadTasks.Count);
    }
}