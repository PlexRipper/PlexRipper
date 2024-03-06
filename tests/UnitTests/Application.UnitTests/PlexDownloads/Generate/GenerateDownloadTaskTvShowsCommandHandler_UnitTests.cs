using DownloadManager.Contracts;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.Validators;

namespace PlexRipper.Application.UnitTests;

public class GenerateDownloadTaskTvShowsCommandHandler_UnitTests : BaseUnitTest<GenerateDownloadTaskTvShowsCommandHandler>
{
    private DownloadTaskTvShowValidator validator = new();

    public GenerateDownloadTaskTvShowsCommandHandler_UnitTests(ITestOutputHelper output) : base(output) { }

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

        foreach (var downloadTaskTvShow in downloadTaskTvShows)
        {
            downloadTaskTvShow.Id.ShouldNotBe(Guid.Empty);
            downloadTaskTvShow.Key.ShouldBeGreaterThan(0);
            downloadTaskTvShow.Title.ShouldNotBeEmpty();
            downloadTaskTvShow.FullTitle.ShouldNotBeEmpty();
            downloadTaskTvShow.Year.ShouldBeGreaterThan(0);
            downloadTaskTvShow.DataTotal.ShouldBeGreaterThan(0);
            downloadTaskTvShow.DownloadStatus.ShouldBe(DownloadStatus.Queued);
            downloadTaskTvShow.CreatedAt.ShouldBe(DateTime.Now, TimeSpan.FromSeconds(5));

            downloadTaskTvShow.PlexServerId.ShouldBe(1);
            downloadTaskTvShow.PlexLibraryId.ShouldBe(1);

            downloadTaskTvShow.DownloadTaskType.ShouldBe(DownloadTaskType.TvShow);
            downloadTaskTvShow.MediaType.ShouldBe(PlexMediaType.TvShow);
            downloadTaskTvShow.Children.ShouldBeEmpty();
        }
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