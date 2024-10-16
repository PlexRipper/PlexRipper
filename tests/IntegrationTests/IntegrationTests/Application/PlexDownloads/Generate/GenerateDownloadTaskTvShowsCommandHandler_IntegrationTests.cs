using Application.Contracts;
using Data.Contracts;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Domain.Validators;

namespace IntegrationTests;

public class GenerateDownloadTaskTvShowsCommandHandler_IntegrationTests : BaseIntegrationTests
{
    private DownloadTaskTvShowValidator validator = new();

    public GenerateDownloadTaskTvShowsCommandHandler_IntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldHaveGeneratedAllTvShowsDownloadTasks_WhenGivenValidCommands()
    {
        // Arrange
        using var container = await CreateContainer(config =>
        {
            config.DatabaseOptions = x =>
            {
                x.PlexServerCount = 1;
                x.PlexLibraryCount = 1;
                x.TvShowCount = 5;
                x.TvShowSeasonCount = 3;
                x.TvShowEpisodeCount = 3;
            };
        });

        var plexTvShows = await container.DbContext.PlexTvShows.ToListAsync();

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
        var mediatr = container.Mediator;
        var result = await mediatr.Send(new GenerateDownloadTaskTvShowsCommand(tvShows));

        // Assert
        var downloadTaskTvShows = await container.DbContext.DownloadTaskTvShow.IncludeAll().ToListAsync();

        downloadTaskTvShows.Count.ShouldBe(5);
        result.IsSuccess.ShouldBeTrue();

        foreach (var downloadTaskTvShow in downloadTaskTvShows)
        {
            downloadTaskTvShow.Calculate();
            var validationResult = await validator.ValidateAsync(downloadTaskTvShow);

            // Ignore DownloadDirectory and DestinationDirectory errors as these are set in the DownloadJob
            var validErrors = validationResult.Errors.FindAll(x =>
                !x.PropertyName.Contains(nameof(DownloadTaskFileBase.DownloadDirectory))
                && !x.PropertyName.Contains(nameof(DownloadTaskFileBase.DestinationDirectory))
            );
            validErrors.ShouldBeEmpty();
        }
    }
}
