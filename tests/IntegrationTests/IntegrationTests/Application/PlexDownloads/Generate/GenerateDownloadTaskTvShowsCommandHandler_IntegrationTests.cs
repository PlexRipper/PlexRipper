using Data.Contracts;
using DownloadManager.Contracts;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Domain.Validators;

namespace IntegrationTests;

[Collection("Sequential")]
public class GenerateDownloadTaskTvShowsCommandHandler_IntegrationTests : BaseIntegrationTests
{
    private DownloadTaskTvShowValidator validator = new();

    public GenerateDownloadTaskTvShowsCommandHandler_IntegrationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldHaveGeneratedAllTvShowsDownloadTasks_WhenGivenValidCommands()
    {
        // Arrange
        await CreateContainer();
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.TvShowCount = 5;
            config.TvShowSeasonCount = 3;
            config.TvShowEpisodeCount = 3;
        });

        var plexTvShows = await Container.PlexRipperDbContext.PlexTvShows.ToListAsync();

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

        // Act
        var mediatr = Container.Mediator;
        var result = await mediatr.Send(new GenerateDownloadTaskTvShowsCommand(tvShows));

        // Assert
        var downloadTaskTvShows = await Container
            .PlexRipperDbContext
            .DownloadTaskTvShow.IncludeAll()
            .ToListAsync();

        downloadTaskTvShows.Count.ShouldBe(5);
        result.IsSuccess.ShouldBeTrue();

        foreach (var downloadTaskTvShow in downloadTaskTvShows)
            (await validator.ValidateAsync(downloadTaskTvShow)).Errors.ShouldBeEmpty();
    }
}