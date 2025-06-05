using Application.Contracts;
using Data.Contracts;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.Validators;

namespace PlexRipper.Application.UnitTests;

public class DownloadTaskFactoryGenerateTvShowEpisodesDownloadTasksAsyncUnitTests
    : BaseCommandUnitTest<GenerateDownloadTaskTvShowEpisodesCommand>
{
    private DownloadTaskTvShowValidator validator = new();

    public DownloadTaskFactoryGenerateTvShowEpisodesDownloadTasksAsyncUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenPlexTvShowsAreEmpty()
    {
        // Arrange
        await SetupDatabase(84901);
        var downloadMediaDtos = new List<DownloadMediaDTO>();

        // Act
        var command = new GenerateDownloadTaskTvShowEpisodesCommand(downloadMediaDtos);
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldGenerateValidTvShowDownloadTaskWithEpisodeDownloadTask_WhenNoDownloadTasksExist()
    {
        // Arrange
        await SetupDatabase(
            47893,
            config =>
            {
                config.TvShowCount = 5;
                config.TvShowSeasonCount = 5;
                config.TvShowEpisodeCount = 5;
            }
        );

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
        var request = new CreateDownloadTasksRequest(downloadMediaDtos, 99);
        var command = new GenerateDownloadTaskTvShowEpisodesCommand(request);
        var result = await TestHandlerExecuteAsync(command);

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

        foreach (var episodeFile in downloadTaskEpisodeFiles)
        {
            episodeFile.DestinationFolderPathId.ShouldBe(99);
        }
    }

    [Fact]
    public async Task ShouldGenerateValidEpisodeDownloadTask_WhenTvShowParentDownloadTaskAlreadyExist()
    {
        // Arrange
        await SetupDatabase(
            52051,
            config =>
            {
                config.TvShowCount = 5;
                config.TvShowSeasonCount = 5;
                config.TvShowEpisodeCount = 5;
            }
        );
        var dbContext = IDbContext;
        var plexTvShows = await dbContext.PlexTvShows.IncludeAll().ToListAsync();
        var plexEpisodes = plexTvShows.SelectMany(x => x.Seasons).SelectMany(x => x.Episodes).ToList();

        // Create a download task for the TV-show
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
        var result = await TestHandlerExecuteAsync(command);

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

        downloadTaskTvShows.FirstOrDefault(x => x.Id == createdTvShowDownloadTask.Id).ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldIgnoreNonExistentEpisodeIds_WhenProcessingValidEpisodes()
    {
        // Arrange
        await SetupDatabase(
            34567,
            config =>
            {
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 3;
            }
        );

        var plexTvShows = await IDbContext
            .PlexTvShows.Include(x => x.Seasons)
            .ThenInclude(x => x.Episodes)
            .ToListAsync();

        var plexEpisodes = plexTvShows.SelectMany(x => x.Seasons).SelectMany(x => x.Episodes).ToList();
        var validEpisodeIds = plexEpisodes.Select(x => x.Id).ToList();
        var invalidEpisodeIds = new List<int> { 99999, 88888, 77777 }; // Non-existent IDs

        var downloadMediaDtos = new List<DownloadMediaDTO>
        {
            new()
            {
                Type = PlexMediaType.Episode,
                MediaIds = validEpisodeIds.Concat(invalidEpisodeIds).ToList(),
                PlexServerId = 1,
                PlexLibraryId = 1,
            },
        };

        // Act
        var command = new GenerateDownloadTaskTvShowEpisodesCommand(downloadMediaDtos);
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue(result.ToString());
        var downloadTaskTvShows = await IDbContext.DownloadTaskTvShow.IncludeAll().ToListAsync();
        downloadTaskTvShows.Count.ShouldBe(1);

        var downloadTaskEpisodes = downloadTaskTvShows.SelectMany(x => x.Children).SelectMany(x => x.Children).ToList();
        downloadTaskEpisodes.Count.ShouldBe(3); // Only valid episodes should be processed
    }

    [Fact]
    public async Task ShouldHandleDuplicateEpisodeIds_WithoutCreatingDuplicateDownloadTasks()
    {
        // Arrange
        await SetupDatabase(
            45678,
            config =>
            {
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 2;
            }
        );

        var plexTvShows = await IDbContext
            .PlexTvShows.Include(x => x.Seasons)
            .ThenInclude(x => x.Episodes)
            .ToListAsync();

        var plexEpisodes = plexTvShows.SelectMany(x => x.Seasons).SelectMany(x => x.Episodes).ToList();
        var episodeIds = plexEpisodes.Select(x => x.Id).ToList();
        var duplicatedEpisodeIds = episodeIds.Concat(episodeIds).ToList(); // Duplicate all IDs

        var downloadMediaDtos = new List<DownloadMediaDTO>
        {
            new()
            {
                Type = PlexMediaType.Episode,
                MediaIds = duplicatedEpisodeIds,
                PlexServerId = 1,
                PlexLibraryId = 1,
            },
        };

        // Act
        var command = new GenerateDownloadTaskTvShowEpisodesCommand(downloadMediaDtos);
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue(result.ToString());
        var downloadTaskTvShows = await IDbContext.DownloadTaskTvShow.IncludeAll().ToListAsync();
        downloadTaskTvShows.Count.ShouldBe(1);

        var downloadTaskEpisodes = downloadTaskTvShows.SelectMany(x => x.Children).SelectMany(x => x.Children).ToList();
        downloadTaskEpisodes.Count.ShouldBe(2); // Should not create duplicates
    }

    [Fact]
    public async Task ShouldCreateNewDownloadTasks_WhenSomeEpisodesAlreadyHaveDownloadTasks()
    {
        // Arrange
        await SetupDatabase(
            56789,
            config =>
            {
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 2;
                config.TvShowEpisodeCount = 3;
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var plexTvShow = await dbContext.PlexTvShows.IncludeAll().FirstOrDefaultAsync();
        plexTvShow.ShouldNotBeNull();
        var plexEpisodes = plexTvShow.Seasons.SelectMany(x => x.Episodes).ToList();
        plexEpisodes.Count.ShouldBe(6);

        // Create a complete download task hierarchy for the first episode only
        var tvShowDownloadTask = await dbContext.DownloadTaskTvShow.AsTracking().IncludeAll().FirstOrDefaultAsync();
        tvShowDownloadTask.ShouldNotBeNull();

        // Copy tv-show key over to the download task
        UpdateInitProperty(tvShowDownloadTask, nameof(tvShowDownloadTask.Key), plexTvShow.Key);

        // Copy season key over to the download task
        var season = plexTvShow.Seasons.FirstOrDefault();
        var seasonDownloadTask = tvShowDownloadTask.Children.FirstOrDefault();
        season.ShouldNotBeNull();
        seasonDownloadTask.ShouldNotBeNull();

        UpdateInitProperty(seasonDownloadTask, nameof(seasonDownloadTask.Key), season.Key);

        // Copy episode key over to the download task
        var episode = season.Episodes.FirstOrDefault();
        var episodeDownloadTask = seasonDownloadTask.Children.FirstOrDefault();
        episode.ShouldNotBeNull();
        episodeDownloadTask.ShouldNotBeNull();
        UpdateInitProperty(episodeDownloadTask, nameof(episodeDownloadTask.Key), episode.Key);

        await dbContext.SaveChangesAsync();

        // Act
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
        var command = new GenerateDownloadTaskTvShowEpisodesCommand(downloadMediaDtos);
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue(result.ToString());

        dbContext = IDbContext;
        dbContext.DownloadTaskTvShow.Count().ShouldBe(1);
        dbContext.DownloadTaskTvShowSeason.Count().ShouldBe(2);
        dbContext.DownloadTaskTvShowEpisode.Count().ShouldBe(6);
        dbContext.DownloadTaskTvShowEpisodeFile.Count().ShouldBe(7);

        var downloadTaskEpisodes = await dbContext.DownloadTaskTvShowEpisode.ToListAsync();

        // Verify the existing episode download task still exists
        downloadTaskEpisodes.ShouldContain(x => x.Key == episodeDownloadTask.Key);
    }

    [Fact]
    public async Task ShouldGenerateDownloadTasksForMultipleTvShows_WhenEpisodesFromDifferentShowsRequested()
    {
        // Arrange
        await SetupDatabase(
            67890,
            config =>
            {
                config.TvShowCount = 3;
                config.TvShowSeasonCount = 2;
                config.TvShowEpisodeCount = 2;
            }
        );

        var plexTvShows = await IDbContext
            .PlexTvShows.Include(x => x.Seasons)
            .ThenInclude(x => x.Episodes)
            .ToListAsync();

        plexTvShows.Count.ShouldBe(3);

        // Select episodes from all 3 TV shows
        var plexEpisodes = plexTvShows.SelectMany(x => x.Seasons).SelectMany(x => x.Episodes).ToList();
        plexEpisodes.Count.ShouldBe(12); // 3 shows × 2 seasons × 2 episodes each

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
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue(result.ToString());
        var downloadTaskTvShows = await IDbContext.DownloadTaskTvShow.IncludeAll().ToListAsync();
        downloadTaskTvShows.Count.ShouldBe(3); // One for each TV show

        var downloadTaskSeasons = downloadTaskTvShows.SelectMany(x => x.Children).ToList();
        downloadTaskSeasons.Count.ShouldBe(6); // 3 shows × 2 seasons each

        var downloadTaskEpisodes = downloadTaskSeasons.SelectMany(x => x.Children).ToList();
        downloadTaskEpisodes.Count.ShouldBe(12); // 3 shows × 2 seasons × 2 episodes each

        var downloadTaskEpisodeFiles = downloadTaskEpisodes.SelectMany(x => x.Children).ToList();
        downloadTaskEpisodeFiles.Count.ShouldBe(12); // One file per episode

        // Verify each TV show has correct structure
        foreach (var tvShow in downloadTaskTvShows)
        {
            tvShow.Children.Count.ShouldBe(2); // 2 seasons per show
            foreach (var season in tvShow.Children)
            {
                season.Children.Count.ShouldBe(2); // 2 episodes per season
            }
        }
    }

    [Fact]
    public async Task ShouldIgnoreNonEpisodeMediaTypes_WhenMixedMediaTypesProvided()
    {
        // Arrange
        await SetupDatabase(
            78901,
            config =>
            {
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 2;
                config.MovieCount = 2;
            }
        );

        var plexTvShows = await IDbContext
            .PlexTvShows.Include(x => x.Seasons)
            .ThenInclude(x => x.Episodes)
            .ToListAsync();
        var plexMovies = await IDbContext.PlexMovies.ToListAsync();

        plexTvShows.Count.ShouldBe(1);
        plexMovies.Count.ShouldBe(2);

        var plexEpisodes = plexTvShows.SelectMany(x => x.Seasons).SelectMany(x => x.Episodes).ToList();
        plexEpisodes.Count.ShouldBe(2);

        var downloadMediaDtos = new List<DownloadMediaDTO>
        {
            new()
            {
                Type = PlexMediaType.Episode,
                MediaIds = plexEpisodes.Select(x => x.Id).ToList(),
                PlexServerId = 1,
                PlexLibraryId = 1,
            },
            new()
            {
                Type = PlexMediaType.Movie, // This should be ignored by the episode handler
                MediaIds = plexMovies.Select(x => x.Id).ToList(),
                PlexServerId = 1,
                PlexLibraryId = 1,
            },
        };

        // Act
        var command = new GenerateDownloadTaskTvShowEpisodesCommand(downloadMediaDtos);
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue(result.ToString());
        var downloadTaskTvShows = await IDbContext.DownloadTaskTvShow.IncludeAll().ToListAsync();
        downloadTaskTvShows.Count.ShouldBe(1); // Only TV show download tasks should be created

        var downloadTaskEpisodes = downloadTaskTvShows.SelectMany(x => x.Children).SelectMany(x => x.Children).ToList();
        downloadTaskEpisodes.Count.ShouldBe(2); // Only episode download tasks should be created

        // Verify no movie download tasks were created
        var movieDownloadTasks = await IDbContext.DownloadTaskMovie.ToListAsync();
        movieDownloadTasks.Count.ShouldBe(0);
    }

    [Fact]
    public async Task ShouldSetCorrectDestinationFolderPathId_WhenCustomDestinationFolderProvided()
    {
        // Arrange
        await SetupDatabase(
            89012,
            config =>
            {
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 2;
            }
        );

        var plexTvShows = await IDbContext
            .PlexTvShows.Include(x => x.Seasons)
            .ThenInclude(x => x.Episodes)
            .ToListAsync();

        var plexEpisodes = plexTvShows.SelectMany(x => x.Seasons).SelectMany(x => x.Episodes).ToList();
        plexEpisodes.Count.ShouldBe(2);

        var customDestinationFolderId = 12345;

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
        var request = new CreateDownloadTasksRequest(downloadMediaDtos, customDestinationFolderId);
        var command = new GenerateDownloadTaskTvShowEpisodesCommand(request);
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue(result.ToString());
        var downloadTaskEpisodeFiles = await IDbContext.DownloadTaskTvShowEpisodeFile.ToListAsync();
        downloadTaskEpisodeFiles.Count.ShouldBe(2);

        foreach (var episodeFile in downloadTaskEpisodeFiles)
        {
            episodeFile.DestinationFolderPathId.ShouldBe(customDestinationFolderId);
        }
    }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenEmptyMediaIdsList()
    {
        // Arrange
        await SetupDatabase(90123);

        var downloadMediaDtos = new List<DownloadMediaDTO>
        {
            new()
            {
                Type = PlexMediaType.Episode,
                MediaIds = new List<int>(), // Empty list
                PlexServerId = 1,
                PlexLibraryId = 1,
            },
        };

        // Act
        var command = new GenerateDownloadTaskTvShowEpisodesCommand(downloadMediaDtos);
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCalculateDownloadTaskPropertiesCorrectly_WhenDownloadTasksGenerated()
    {
        // Arrange
        await SetupDatabase(
            91234,
            config =>
            {
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 3;
            }
        );

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
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue(result.ToString());
        var downloadTaskTvShows = await IDbContext.DownloadTaskTvShow.IncludeAll().ToListAsync();

        foreach (var tvShow in downloadTaskTvShows)
        {
            tvShow.Calculate();

            // Verify calculated properties
            tvShow.DataReceived.ShouldBeGreaterThanOrEqualTo(0);
            tvShow.DataTotal.ShouldBeGreaterThan(0);
            tvShow.FileTransferSpeed.ShouldBeGreaterThanOrEqualTo(0);
            tvShow.Percentage.ShouldBeGreaterThanOrEqualTo(0);
            tvShow.Percentage.ShouldBeLessThanOrEqualTo(100);

            // Verify parent-child relationships
            foreach (var season in tvShow.Children)
            {
                season.ParentId.ShouldBe(tvShow.Id);
                foreach (var episode in season.Children)
                {
                    episode.ParentId.ShouldBe(season.Id);
                    foreach (var episodeFile in episode.Children)
                    {
                        episodeFile.ParentId.ShouldBe(episode.Id);
                    }
                }
            }
        }
    }
}
