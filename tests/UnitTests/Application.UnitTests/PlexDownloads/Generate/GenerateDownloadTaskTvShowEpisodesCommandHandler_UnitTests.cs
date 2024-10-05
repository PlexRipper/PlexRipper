using Application.Contracts;
using Data.Contracts;
using PlexRipper.Domain.Validators;

namespace PlexRipper.Application.UnitTests;

public class DownloadTaskFactory_GenerateTvShowEpisodesDownloadTasksAsync_UnitTests
    : BaseUnitTest<GenerateDownloadTaskTvShowEpisodesCommandHandler>
{
    private DownloadTaskTvShowValidator validator = new();

    public DownloadTaskFactory_GenerateTvShowEpisodesDownloadTasksAsync_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenPlexTvShowsAreEmpty()
    {
        // Arrange
        await SetupDatabase();
        var downloadMediaDtos = new List<DownloadMediaDTO>();

        // Act
        var command = new GenerateDownloadTaskTvShowEpisodesCommand(downloadMediaDtos);
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldGenerateValidTvShowDownloadTaskWithEpisodeDownloadTask_WhenNoDownloadTasksExist()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.TvShowCount = 5;
            config.TvShowSeasonCount = 5;
            config.TvShowEpisodeCount = 5;
        });

        var plexTvShows = await IDbContext
            .PlexTvShows.Include(x => x.Seasons)
            .ThenInclude(x => x.Episodes)
            .ToListAsync();

        var plexEpisodes = plexTvShows.SelectMany(x => x.Seasons).SelectMany(x => x.Episodes).ToList();
        var downloadMediaDtos = new List<DownloadMediaDTO>
        {
            new()
            {
                Type = PlexMediaType.Episode,
                MediaIds = plexEpisodes.Select(x => x.Id).ToList(),
                PlexServerId = 1,
                PlexLibraryId = 1,
            },
        };

        // Act
        var command = new GenerateDownloadTaskTvShowEpisodesCommand(downloadMediaDtos);
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue(result.ToString());
        var downloadTaskTvShows = await IDbContext.DownloadTaskTvShow.IncludeAll().ToListAsync();
        downloadTaskTvShows.Count.ShouldBe(5);

        var downloadTaskSeasons = downloadTaskTvShows.SelectMany(x => x.Children).ToList();
        downloadTaskSeasons.Count.ShouldBe(25);

        var downloadTaskEpisodes = downloadTaskSeasons.SelectMany(x => x.Children).ToList();
        downloadTaskEpisodes.Count.ShouldBe(125);

        var downloadTaskEpisodeFiles = downloadTaskEpisodes.SelectMany(x => x.Children).ToList();
        downloadTaskEpisodeFiles.Count.ShouldBe(125);

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

    [Fact]
    public async Task ShouldGenerateValidEpisodeDownloadTask_WhenTvShowParentDownloadTaskAlreadyExist()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.TvShowCount = 5;
            config.TvShowSeasonCount = 5;
            config.TvShowEpisodeCount = 5;
        });
        var dbContext = IDbContext;
        var plexTvShows = await dbContext.PlexTvShows.IncludeAll().ToListAsync();
        var plexEpisodes = plexTvShows.SelectMany(x => x.Seasons).SelectMany(x => x.Episodes).ToList();

        // Create a download task for the tv show
        var createdTvShowDownloadTask = plexTvShows.First().MapToDownloadTask();
        dbContext.DownloadTaskTvShow.Add(createdTvShowDownloadTask);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var downloadMediaDtos = new List<DownloadMediaDTO>
        {
            new()
            {
                Type = PlexMediaType.Episode,
                MediaIds = plexEpisodes.Select(x => x.Id).ToList(),
                PlexServerId = 1,
                PlexLibraryId = 1,
            },
        };

        // Act
        var command = new GenerateDownloadTaskTvShowEpisodesCommand(downloadMediaDtos);
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue(result.ToString());
        var downloadTaskTvShows = await IDbContext.DownloadTaskTvShow.IncludeAll().ToListAsync();
        downloadTaskTvShows.Count.ShouldBe(5);
        downloadTaskTvShows.FirstOrDefault(x => x.Id == createdTvShowDownloadTask.Id).ShouldNotBeNull();
    }
}
