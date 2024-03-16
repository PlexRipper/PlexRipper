using Application.Contracts;
using Data.Contracts;
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
            config.TvShowCount = 5;
            config.TvShowSeasonCount = 3;
            config.TvShowEpisodeCount = 3;
        });
        var plexTvShows = await IDbContext.PlexTvShows.IncludeAll().ToListAsync();

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
        var downloadTaskTvShows = await IDbContext.GetAllDownloadTasksAsync();

        downloadTaskTvShows.Count.ShouldBe(5);

        foreach (var downloadTaskTvShow in downloadTaskTvShows)
        {
            downloadTaskTvShow.Id.ShouldNotBe(Guid.Empty);
            downloadTaskTvShow.Key.ShouldBeGreaterThan(0);
            downloadTaskTvShow.Title.ShouldNotBeEmpty();
            downloadTaskTvShow.FullTitle.ShouldNotBeEmpty();
            downloadTaskTvShow.DownloadStatus.ShouldBe(DownloadStatus.Queued);

            downloadTaskTvShow.PlexServerId.ShouldBe(1);
            downloadTaskTvShow.PlexLibraryId.ShouldBe(1);

            downloadTaskTvShow.DownloadTaskType.ShouldBe(DownloadTaskType.TvShow);
            downloadTaskTvShow.MediaType.ShouldBe(PlexMediaType.TvShow);
            downloadTaskTvShow.Children.ShouldBeEmpty();
        }
    }
}