using DownloadManager.Contracts;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application.UnitTests;

public class DownloadTaskFactory_GenerateTvShowDownloadTasksAsync_UnitTests : BaseUnitTest<GenerateDownloadTaskTvShowsCommandHandler>
{
    public DownloadTaskFactory_GenerateTvShowDownloadTasksAsync_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldHaveInsertedValidDownloadTaskTvShowsInDatabase_WhenGivenValidPlexTvShows()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.TvShowCount = 5;
        });
        var plexTvShows = await IDbContext.PlexTvShows.ToListAsync();

        var tvShows = new List<DownloadMediaDTO>
        {
            new()
            {
                Type = PlexMediaType.TvShow,
                MediaIds = plexTvShows.Select(x => x.Id).ToList(),
                PlexServerId = 1,
                PlexLibraryId = 1,
            },
        };

        mock.SetupMediator(It.IsAny<GenerateDownloadTaskTvShowSeasonsCommand>).ReturnsAsync(Result.Ok());

        // Act
        var command = new GenerateDownloadTaskTvShowsCommand(tvShows);
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        ResetDbContext();
        var downloadTaskTvShows = await IDbContext
            .DownloadTaskTvShow
            .ToListAsync();

        downloadTaskTvShows.Count.ShouldBe(5);

        // foreach (var downloadTaskMovie in plexDownloadTaskMovies)
        //     (await validator.ValidateAsync(downloadTaskMovie)).Errors.ShouldBeEmpty();
    }

//     [Fact]
//     public async Task ShouldGenerateValidTvShowDownloadTaskWithAllEpisodesDownloadTask_WhenNoDownloadTasksExist()
//     {
//         // Arrange
//         await SetupDatabase(config =>
//         {
//             config.PlexServerCount = 1;
//             config.PlexLibraryCount = 1;
//             config.TvShowCount = 5;
//             config.TvShowSeasonCount = 2;
//             config.TvShowEpisodeCount = 5;
//         });
//
//         var tvShows = DbContext.PlexTvShows.IncludeEpisodes().IncludePlexServer().IncludePlexLibrary().ToList();
//
//         mock.SetupMediator(It.IsAny<GetPlexTvShowByIdWithEpisodesQuery>)
//             .ReturnsAsync((GetPlexTvShowByIdWithEpisodesQuery query, CancellationToken _) => Result.Ok(tvShows.Find(x => x.Id == query.PlexTvShowId)));
//
//         var tvShowIds = new List<int> { 1 };
//
//         // Act
//         var result = await _sut.GenerateTvShowDownloadTasksAsync(tvShowIds);
//
//         // Assert
//         result.IsSuccess.ShouldBeTrue();
//         result.Value.Count.ShouldBe(tvShowIds.Count);
//         var tvShowDownloadTask = result.Value.First();
//
//         ShouldDownloadTask.ShouldTvShow(tvShowDownloadTask, tvShows[0]);
//     }
}