using Application.Contracts;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;

namespace Data.UnitTests;

public class GetDownloadPreviewQueryHandler_UnitTests : BaseUnitTest<GetDownloadPreviewQueryHandler>
{
    public GetDownloadPreviewQueryHandler_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldReturnNoDownloadPreview_WhenEmptyListIsGiven()
    {
        // Arrange
        await SetupDatabase();

        var request = new GetDownloadPreviewQuery(new List<DownloadMediaDTO>());

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldReturnTheCorrectDownloadPreview_WhenMixedMediaTypes()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.TvShowCount = 5;
            config.TvShowSeasonCount = 5;
            config.TvShowEpisodeCount = 5;
        });

        var tvShows = await DbContext.PlexTvShows
            .Include(x => x.Seasons)
            .ThenInclude(x => x.Episodes)
            .AsNoTracking()
            .ToListAsync();

        tvShows.Count.ShouldBe(5);
        tvShows.SelectMany(x => x.Seasons).ToList().Count.ShouldBe(5 * 5);
        tvShows.SelectMany(x => x.Seasons.SelectMany(y => y.Episodes)).ToList().Count.ShouldBe(5 * 5 * 5);

        var downloadMedia = new List<DownloadMediaDTO>();

        downloadMedia.Add(new DownloadMediaDTO
        {
            MediaIds = tvShows.GetRange(0, 2).Select(x => x.Id).ToList(),
            Type = PlexMediaType.TvShow,
            PlexServerId = 1,
        });

        downloadMedia.Add(new DownloadMediaDTO
        {
            MediaIds = tvShows[3].Seasons.GetRange(0, 3).Select(x => x.Id).ToList(),
            Type = PlexMediaType.Season,
            PlexServerId = 1,
        });

        downloadMedia.Add(new DownloadMediaDTO
        {
            MediaIds = tvShows[4].Seasons[2].Episodes.GetRange(1, 4).Select(x => x.Id).ToList(),
            Type = PlexMediaType.Episode,
            PlexServerId = 1,
        });

        var request = new GetDownloadPreviewQuery(downloadMedia);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;

        value.ShouldNotBeEmpty();
        value.Count.ShouldBe(4);

        // Full tvShows should have been added
        for (var i = 0; i < 1; i++)
        {
            value[i].ShouldNotBeNull();
            value[i].Children.Count.ShouldBe(5);
            value[i].Children.ShouldAllBe(x => x.Children.Count == 5);
        }

        // Seasons check
        value[2].Children.Count.ShouldBe(3);
        value[2].Children.ShouldAllBe(x => x.Children.Count == 5);

        // Loose episodes
        value[3].Children.Count.ShouldBe(1);
        value[3].Children[0].Children.Count.ShouldBe(4);
    }
}