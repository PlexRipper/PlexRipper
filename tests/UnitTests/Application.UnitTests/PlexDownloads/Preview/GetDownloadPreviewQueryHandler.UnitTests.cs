using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.BaseTests;

namespace Reaparr.Application.UnitTests;

public class GetDownloadPreviewQueryHandlerUnitTests : BaseUnitTest<GetDownloadPreviewQueryHandler>
{
    public GetDownloadPreviewQueryHandlerUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnNoDownloadPreview_WhenEmptyListIsGiven()
    {
        // Arrange
        await SetupDatabase(15140);

        var request = new GetDownloadPreviewQuery(new List<DownloadMediaDTO>());

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldReturnTheCorrectDownloadPreview_WhenMixedMediaTypes()
    {
        // Arrange
        await SetupDatabase(
            47561,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.TvShowCount = 5;
                config.TvShowSeasonCount = 5;
                config.TvShowEpisodeCount = 5;
            }
        );

        var tvShows = await IDbContext
            .PlexTvShows.Include(x => x.Seasons)
            .ThenInclude(x => x.Episodes)
            .AsNoTracking()
            .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);

        tvShows.Count.ShouldBe(5);

        // Verify the test data is set up correctly
        var actualSeasonsPerShow = tvShows.First().Seasons.Count;
        var actualEpisodesPerSeason = tvShows.First().Seasons.First().Episodes.Count;

        var downloadMedia = new List<DownloadMediaDTO>();

        downloadMedia.Add(
            new DownloadMediaDTO
            {
                MediaIds = tvShows.GetRange(0, 2).Select(x => x.Id).ToList(),
                Type = PlexMediaType.TvShow,
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            }
        );

        downloadMedia.Add(
            new DownloadMediaDTO
            {
                MediaIds = tvShows[3]
                    .Seasons.ToList()
                    .GetRange(0, Math.Min(3, actualSeasonsPerShow))
                    .Select(x => x.Id)
                    .ToList(),
                Type = PlexMediaType.Season,
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            }
        );

        downloadMedia.Add(
            new DownloadMediaDTO
            {
                MediaIds = tvShows[4]
                    .Seasons.ElementAt(Math.Min(2, actualSeasonsPerShow - 1))
                    .Episodes.Skip(1)
                    .Take(Math.Min(4, actualEpisodesPerSeason - 1))
                    .Select(x => x.Id)
                    .ToList(),
                Type = PlexMediaType.Episode,
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            }
        );

        var request = new GetDownloadPreviewQuery(downloadMedia);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;

        value.ShouldNotBeEmpty();
        value.Count.ShouldBe(4);

        // Full tvShows should have been added (first 2 results should be TV shows)
        // First TV show: 3 seasons with 5 episodes each
        value[0].ShouldNotBeNull();
        value[0].Children.Count.ShouldBe(3);
        value[0].Children.ShouldAllBe(x => x.Children.Count == 5);

        // Second TV show: 1 season with 4 episodes
        value[1].ShouldNotBeNull();
        value[1].Children.Count.ShouldBe(1);
        value[1].Children.ShouldAllBe(x => x.Children.Count == 4);

        // Seasons check (seasons from 4th TV show): 5 seasons with 5 episodes each
        value[2].Children.Count.ShouldBe(5);
        value[2].Children.ShouldAllBe(x => x.Children.Count == 5);

        // Loose episodes (episodes from 5th TV show): 5 seasons with 5 episodes each
        value[3].Children.Count.ShouldBe(5);
        value[3].Children.ShouldAllBe(x => x.Children.Count == 5);
    }

    #region Movie Tests

    [Fact]
    public async Task ShouldReturnMoviePreview_WhenMoviesWithoutQualitiesRequested()
    {
        // Arrange
        await SetupDatabase(
            12345,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 3;
            }
        );

        var movies = await IDbContext
            .PlexMovies.AsNoTracking()
            .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);
        movies.Count.ShouldBe(3);

        var downloadMedia = new List<DownloadMediaDTO>
        {
            new()
            {
                MediaIds = movies.Select(x => x.Id).ToList(),
                Type = PlexMediaType.Movie,
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
        };

        var request = new GetDownloadPreviewQuery(downloadMedia);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.Count.ShouldBe(3);
        value.ShouldAllBe(x => x.MediaType == PlexMediaType.Movie);
        value.ShouldAllBe(x => x.Children.Count == 0);
        value.ShouldAllBe(x => x.Size > 0);
    }

    [Fact]
    public async Task ShouldReturnMoviePreview_WhenMoviesWithSpecificQualitiesRequested()
    {
        // Arrange
        await SetupDatabase(
            23456,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 2;
            }
        );

        var movies = await IDbContext
            .PlexMovies.Include(x => x.MediaDataList)
            .AsNoTracking()
            .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);

        var qualities = movies
            .SelectMany(movie =>
                movie.MediaDataList.Select(md => new PlexMediaQualityDTO
                {
                    MediaId = movie.Id,
                    DataId = md.Id,
                    MediaDataType = PlexMediaType.Movie,
                    Quality = VideoQuality.UHD_4K,
                })
            )
            .ToList();

        var downloadMedia = new List<DownloadMediaDTO>
        {
            new()
            {
                MediaIds = movies.Select(x => x.Id).ToList(),
                Type = PlexMediaType.Movie,
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = qualities,
            },
        };

        var request = new GetDownloadPreviewQuery(downloadMedia);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.Count.ShouldBe(2);
        value.ShouldAllBe(x => x.MediaType == PlexMediaType.Movie);
        value.ShouldAllBe(x => x.Size > 0);
    }

    [Fact]
    public async Task ShouldReturnMoviePreview_WhenMixedMoviesWithAndWithoutQualitiesRequested()
    {
        // Arrange
        await SetupDatabase(
            34567,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 4;
            }
        );

        var movies = await IDbContext
            .PlexMovies.Include(x => x.MediaDataList)
            .AsNoTracking()
            .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);

        var moviesWithQuality = movies.Take(2).ToList();

        var qualities = moviesWithQuality
            .SelectMany(movie =>
                movie
                    .MediaDataList.Take(1)
                    .Select(md => new PlexMediaQualityDTO
                    {
                        MediaId = movie.Id,
                        DataId = md.Id,
                        MediaDataType = PlexMediaType.Movie,
                        Quality = VideoQuality.FullHD,
                    })
            )
            .ToList();

        var downloadMedia = new List<DownloadMediaDTO>
        {
            new()
            {
                MediaIds = movies.Select(x => x.Id).ToList(),
                Type = PlexMediaType.Movie,
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = qualities,
            },
        };

        var request = new GetDownloadPreviewQuery(downloadMedia);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.Count.ShouldBe(4);
        value.ShouldAllBe(x => x.MediaType == PlexMediaType.Movie);
        value.ShouldAllBe(x => x.Size > 0);
    }

    [Fact]
    public async Task ShouldReturnEmptyList_WhenMoviesWithEmptyMediaIdsRequested()
    {
        // Arrange
        await SetupDatabase(45678);

        var downloadMedia = new List<DownloadMediaDTO>
        {
            new()
            {
                MediaIds = [],
                Type = PlexMediaType.Movie,
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
        };

        var request = new GetDownloadPreviewQuery(downloadMedia);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    #endregion

    #region TV Show Tests

    [Fact]
    public async Task ShouldReturnTvShowPreview_WhenFullTvShowsWithoutQualitiesRequested()
    {
        // Arrange
        await SetupDatabase(
            56789,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.TvShowCount = 2;
                config.TvShowSeasonCount = 3;
                config.TvShowEpisodeCount = 4;
            }
        );

        var tvShows = await IDbContext
            .PlexTvShows.AsNoTracking()
            .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);

        var downloadMedia = new List<DownloadMediaDTO>
        {
            new()
            {
                MediaIds = tvShows.Select(x => x.Id).ToList(),
                Type = PlexMediaType.TvShow,
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
        };

        var request = new GetDownloadPreviewQuery(downloadMedia);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.Count.ShouldBe(2);
        value.ShouldAllBe(x => x.MediaType == PlexMediaType.TvShow);
        value.ShouldAllBe(x => x.Children.Count == 3); // 3 seasons per show
        value.ShouldAllBe(x => x.Children.All(season => season.Children.Count == 4)); // 4 episodes per season
        value.ShouldAllBe(x => x.Size > 0);
        value.ShouldAllBe(x => x.ChildCount == 3);
    }

    [Fact]
    public async Task ShouldReturnSeasonPreview_WhenSeasonsWithoutQualitiesRequested()
    {
        // Arrange
        await SetupDatabase(
            67890,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 3;
                config.TvShowEpisodeCount = 5;
            }
        );

        var seasons = await IDbContext
            .PlexTvShowSeason.AsNoTracking()
            .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);

        var downloadMedia = new List<DownloadMediaDTO>
        {
            new()
            {
                MediaIds = seasons.Take(2).Select(x => x.Id).ToList(),
                Type = PlexMediaType.Season,
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
        };

        var request = new GetDownloadPreviewQuery(downloadMedia);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.Count.ShouldBe(1); // One TV show containing the seasons
        value[0].MediaType.ShouldBe(PlexMediaType.TvShow);
        value[0].Children.Count.ShouldBe(2); // 2 seasons requested
        value[0].Children.ShouldAllBe(x => x.MediaType == PlexMediaType.Season);
        value[0].Children.ShouldAllBe(x => x.Children.Count == 5); // 5 episodes per season
        value[0].Size.ShouldBeGreaterThan(0);
        value[0].ChildCount.ShouldBe(2);
    }

    [Fact]
    public async Task ShouldReturnEpisodePreview_WhenEpisodesWithoutQualitiesRequested()
    {
        // Arrange
        await SetupDatabase(
            78901,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 2;
                config.TvShowEpisodeCount = 6;
            }
        );

        var episodes = await IDbContext
            .PlexTvShowEpisodes.AsNoTracking()
            .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);

        var downloadMedia = new List<DownloadMediaDTO>
        {
            new()
            {
                MediaIds = episodes.Take(3).Select(x => x.Id).ToList(),
                Type = PlexMediaType.Episode,
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
        };

        var request = new GetDownloadPreviewQuery(downloadMedia);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.Count.ShouldBe(1); // One TV show
        value[0].MediaType.ShouldBe(PlexMediaType.TvShow);

        // Should have seasons containing the episodes
        var totalEpisodesInSeasons = value[0].Children.Sum(season => season.Children.Count);
        totalEpisodesInSeasons.ShouldBe(3);

        value[0].Children.ShouldAllBe(x => x.MediaType == PlexMediaType.Season);
        value[0].Size.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldReturnEpisodePreview_WhenEpisodesWithSpecificQualitiesRequested()
    {
        // Arrange
        await SetupDatabase(
            89012,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 3;
            }
        );

        var episodes = await IDbContext
            .PlexTvShowEpisodes.Include(x => x.MediaDataList)
            .AsNoTracking()
            .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);

        var qualities = episodes
            .SelectMany(episode =>
                episode
                    .MediaDataList.Take(1)
                    .Select(md => new PlexMediaQualityDTO
                    {
                        MediaId = episode.Id,
                        DataId = md.Id,
                        MediaDataType = PlexMediaType.Episode,
                        Quality = VideoQuality.HD,
                    })
            )
            .ToList();

        var downloadMedia = new List<DownloadMediaDTO>
        {
            new()
            {
                MediaIds = episodes.Select(x => x.Id).ToList(),
                Type = PlexMediaType.Episode,
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = qualities,
            },
        };

        var request = new GetDownloadPreviewQuery(downloadMedia);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.Count.ShouldBe(1);
        value[0].MediaType.ShouldBe(PlexMediaType.TvShow);

        var totalEpisodes = value[0].Children.Sum(season => season.Children.Count);
        totalEpisodes.ShouldBe(3);

        value[0].Size.ShouldBeGreaterThan(0);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public async Task ShouldReturnEmptyList_WhenInvalidMediaIdsProvided()
    {
        // Arrange
        await SetupDatabase(90123);

        var downloadMedia = new List<DownloadMediaDTO>
        {
            new()
            {
                MediaIds = [999999, 888888], // Non-existent IDs
                Type = PlexMediaType.Movie,
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
        };

        var request = new GetDownloadPreviewQuery(downloadMedia);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldHandleMultipleServersAndLibraries()
    {
        // Arrange
        await SetupDatabase(
            11223,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexMovieLibraryCount = 2;
                config.MovieCount = 2;
            }
        );

        var movies = await IDbContext
            .PlexMovies.AsNoTracking()
            .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);

        var downloadMedia = new List<DownloadMediaDTO>
        {
            new()
            {
                MediaIds = movies.Take(2).Select(x => x.Id).ToList(),
                Type = PlexMediaType.Movie,
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
            new()
            {
                MediaIds = movies.Skip(2).Select(x => x.Id).ToList(),
                Type = PlexMediaType.Movie,
                PlexServerId = 2,
                PlexLibraryId = 2,
                Qualities = [],
            },
        };

        var request = new GetDownloadPreviewQuery(downloadMedia);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.Count.ShouldBe(8); // 2 servers × 2 libraries × 2 movies each = 8 movies
        value.ShouldAllBe(x => x.MediaType == PlexMediaType.Movie);
    }

    [Fact]
    public async Task ShouldReturnCorrectSizeCalculations_WhenTvShowHierarchyBuilt()
    {
        // Arrange
        await SetupDatabase(
            22334,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 2;
                config.TvShowEpisodeCount = 3;
            }
        );

        var tvShows = await IDbContext
            .PlexTvShows.AsNoTracking()
            .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);

        var downloadMedia = new List<DownloadMediaDTO>
        {
            new()
            {
                MediaIds = tvShows.Select(x => x.Id).ToList(),
                Type = PlexMediaType.TvShow,
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
        };

        var request = new GetDownloadPreviewQuery(downloadMedia);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.Count.ShouldBe(1);

        var tvShow = value[0];
        var expectedSize = tvShow.Children.Sum(season => season.Size);
        tvShow.Size.ShouldBe(expectedSize);

        foreach (var season in tvShow.Children)
        {
            var expectedSeasonSize = season.Children.Sum(episode => episode.Size);
            season.Size.ShouldBe(expectedSeasonSize);
            season.ChildCount.ShouldBe(season.Children.Count);
        }

        tvShow.ChildCount.ShouldBe(tvShow.Children.Count);
    }

    [Fact]
    public async Task ShouldReturnSortedResults_WhenMultipleMediaRequested()
    {
        // Arrange
        await SetupDatabase(
            33445,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 5;
                config.TvShowCount = 3;
                config.TvShowSeasonCount = 2;
                config.TvShowEpisodeCount = 2;
            }
        );

        var movies = await IDbContext
            .PlexMovies.AsNoTracking()
            .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);
        var tvShows = await IDbContext
            .PlexTvShows.AsNoTracking()
            .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);

        var downloadMedia = new List<DownloadMediaDTO>
        {
            new()
            {
                MediaIds = movies.Select(x => x.Id).ToList(),
                Type = PlexMediaType.Movie,
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
            new()
            {
                MediaIds = tvShows.Select(x => x.Id).ToList(),
                Type = PlexMediaType.TvShow,
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
        };

        var request = new GetDownloadPreviewQuery(downloadMedia);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.Count.ShouldBe(8); // 5 movies + 3 TV shows

        // Check that results are grouped by media type and contain expected media
        var movieResults = value.Where(x => x.MediaType == PlexMediaType.Movie).ToList();
        var tvShowResults = value.Where(x => x.MediaType == PlexMediaType.TvShow).ToList();

        movieResults.Count.ShouldBe(5); // 5 movies as configured
        tvShowResults.Count.ShouldBe(3); // 3 TV shows as configured

        // Verify all results have valid titles and sizes
        value.ShouldAllBe(x => !string.IsNullOrEmpty(x.Title));
        value.ShouldAllBe(x => x.Size > 0);
    }

    [Fact]
    public async Task ShouldHandleMixedQualitiesAcrossDifferentMediaTypes()
    {
        // Arrange
        await SetupDatabase(
            44556,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 2;
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 2;
            }
        );

        var movies = await IDbContext
            .PlexMovies.Include(x => x.MediaDataList)
            .AsNoTracking()
            .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);

        var episodes = await IDbContext
            .PlexTvShowEpisodes.Include(x => x.MediaDataList)
            .AsNoTracking()
            .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);

        var movieQualities = movies
            .SelectMany(movie =>
                movie
                    .MediaDataList.Take(1)
                    .Select(md => new PlexMediaQualityDTO
                    {
                        MediaId = movie.Id,
                        DataId = md.Id,
                        MediaDataType = PlexMediaType.Movie,
                        Quality = VideoQuality.UHD_4K,
                    })
            )
            .ToList();

        var episodeQualities = episodes
            .SelectMany(episode =>
                episode
                    .MediaDataList.Take(1)
                    .Select(md => new PlexMediaQualityDTO
                    {
                        MediaId = episode.Id,
                        DataId = md.Id,
                        MediaDataType = PlexMediaType.Episode,
                        Quality = VideoQuality.HD,
                    })
            )
            .ToList();

        var downloadMedia = new List<DownloadMediaDTO>
        {
            new()
            {
                MediaIds = movies.Select(x => x.Id).ToList(),
                Type = PlexMediaType.Movie,
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = movieQualities,
            },
            new()
            {
                MediaIds = episodes.Select(x => x.Id).ToList(),
                Type = PlexMediaType.Episode,
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = episodeQualities,
            },
        };

        var request = new GetDownloadPreviewQuery(downloadMedia);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.Count.ShouldBe(3); // 2 movies + 1 TV show

        var movieResults = value.Where(x => x.MediaType == PlexMediaType.Movie).ToList();
        var tvShowResults = value.Where(x => x.MediaType == PlexMediaType.TvShow).ToList();

        movieResults.Count.ShouldBe(2);
        tvShowResults.Count.ShouldBe(1);

        movieResults.ShouldAllBe(x => x.Size > 0);
        tvShowResults.ShouldAllBe(x => x.Size > 0);
    }

    #endregion

    #region Complex Hierarchy Tests

    [Fact]
    public async Task ShouldBuildCorrectHierarchy_WhenMixedTvShowSeasonsAndEpisodesRequested()
    {
        // Arrange
        await SetupDatabase(
            55667,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.TvShowCount = 2;
                config.TvShowSeasonCount = 3;
                config.TvShowEpisodeCount = 4;
            }
        );

        var tvShows = await IDbContext
            .PlexTvShows.Include(x => x.Seasons)
            .ThenInclude(x => x.Episodes)
            .AsNoTracking()
            .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);

        var firstShow = tvShows[0];
        var secondShow = tvShows[1];

        var downloadMedia = new List<DownloadMediaDTO>
        {
            // Full first TV show
            new()
            {
                MediaIds = [firstShow.Id],
                Type = PlexMediaType.TvShow,
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
            // Some seasons from the second show
            new()
            {
                MediaIds = secondShow.Seasons.Take(2).Select(x => x.Id).ToList(),
                Type = PlexMediaType.Season,
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
            // Some episodes from the second show's third season
            new()
            {
                MediaIds = secondShow.Seasons.ElementAt(2).Episodes.Take(2).Select(x => x.Id).ToList(),
                Type = PlexMediaType.Episode,
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
        };

        var request = new GetDownloadPreviewQuery(downloadMedia);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.Count.ShouldBe(2); // 2 TV shows

        // The first show should have all seasons and episodes
        var firstShowResult = value.First(x => x.Id == firstShow.Id);
        firstShowResult.Children.Count.ShouldBe(3); // All 3 seasons
        firstShowResult.Children.ShouldAllBe(x => x.Children.Count == 4); // All 4 episodes per season

        // The second show should have partial content
        var secondShowResult = value.First(x => x.Id == secondShow.Id);
        secondShowResult.Children.Count.ShouldBe(3); // 2 complete seasons + 1 partial season

        // The first two seasons should have all episodes
        secondShowResult.Children.Take(2).ShouldAllBe(x => x.Children.Count == 4);

        // The third season should have only the requested episodes
        secondShowResult.Children.ElementAt(2).Children.Count.ShouldBe(2);
    }

    [Fact]
    public async Task ShouldNotDuplicateEpisodes_WhenSameEpisodeRequestedMultipleTimes()
    {
        // Arrange
        await SetupDatabase(
            66778,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 3;
            }
        );

        var episodes = await IDbContext
            .PlexTvShowEpisodes.AsNoTracking()
            .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);
        var targetEpisode = episodes[0];

        var downloadMedia = new List<DownloadMediaDTO>
        {
            new()
            {
                MediaIds = [targetEpisode.Id],
                Type = PlexMediaType.Episode,
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
            new()
            {
                MediaIds = [targetEpisode.Id], // Same episode requested again
                Type = PlexMediaType.Episode,
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
        };

        var request = new GetDownloadPreviewQuery(downloadMedia);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.Count.ShouldBe(1); // One TV show

        var tvShow = value[0];
        tvShow.Children.Count.ShouldBe(1); // One season
        tvShow.Children[0].Children.Count.ShouldBe(1); // One unique episode (no duplicates)
    }

    #endregion
}
