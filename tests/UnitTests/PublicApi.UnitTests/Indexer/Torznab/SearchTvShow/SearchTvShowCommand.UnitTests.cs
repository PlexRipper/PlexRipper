using Microsoft.EntityFrameworkCore;
using Reaparr.PublicAPI;

namespace PublicApi.UnitTests;

public class SearchTvShowCommandUnitTests : BaseUnitTest<SearchTvShowCommandHandler>
{
    public SearchTvShowCommandUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnPagedEpisodes_WhenNoFiltersProvided()
    {
        // Arrange
        await SetupDatabase(
            1001,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexTvShowLibraryCount = 1;
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 5;
            }
        );

        var offset = 1;
        var limit = 3;
        var cmd = new SearchTvShowCommand
        {
            Query = string.Empty,
            Season = 0,
            Episode = 0,
            Limit = limit,
            Offset = offset,
            IMDB_ID = string.Empty,
            TMDB_ID = 0,
            TVDB_ID = 0,
        };

        var expectedEpisodeTitles = await IDbContext
            .PlexTvShowEpisodes.AsNoTracking()
            .Include(e => e.MediaDataList)
            .OrderBy(e => e.Id)
            .Skip(offset)
            .Take(limit)
            .Select(e => e.MediaDataList.Select(md => md.GetFileName).First())
            .ToListAsync(CancellationToken);

        // Act
        var result = await Sut.ExecuteAsync(cmd, CancellationToken);

        // Assert
        // Response
        result.ShouldNotBeNull();
        result.Channel.ShouldNotBeNull();
        result.Channel.Title.ShouldBe("Reaparr Indexer");
        result.Channel.Description.ShouldBe($"TV Search results for {cmd.Query}");
        result.Channel.Language.ShouldBe("en-us");
        result.Channel.Category.ShouldBe("search");

        result.Channel.Items.ShouldNotBeNull();
        result.Channel.Items.Count.ShouldBe(expectedEpisodeTitles.Count);
        result.Channel.Items.Select(i => i.Title).ToList().ShouldBe(expectedEpisodeTitles);

        // Strict per-item assertions
        foreach (var item in result.Channel.Items)
        {
            item.Guid.ShouldNotBeNull();
            item.Guid.IsPermaLink.ShouldBe("false");
            item.Guid.Value.ShouldBe(item.Link);

            item.Enclosure.ShouldNotBeNull();
            item.Enclosure.Type.ShouldBe("application/x-bittorrent");
            item.Enclosure.Length.ShouldBe(item.Size);

            // Required attributes
            item.Attributes.Any(a => a.Name == "season").ShouldBeTrue();
            item.Attributes.Any(a => a.Name == "episode").ShouldBeTrue();
            item.Attributes.Any(a => a.Name == "type" && a.Value == "series").ShouldBeTrue();
            item.Attributes.Any(a => a.Name == "language" && a.Value == "English").ShouldBeTrue();
            item.Attributes.Any(a => a.Name == "downloadvolumefactor" && a.Value == "0.0").ShouldBeTrue();
            item.Attributes.Any(a => a.Name == "seeders" && int.Parse(a.Value) > 0).ShouldBeTrue();
            item.Attributes.Any(a => a.Name == "peers" && int.Parse(a.Value) > 0).ShouldBeTrue();

            // URL contains expected parameters
            item.Link.ShouldContain(PublicApiRoutes.DownloadTorrent);
            item.Link.ShouldContain("Type=Episode");
            item.Link.ShouldContain("MediaId=");
            item.Link.ShouldContain("DataId=");
            item.Link.ShouldContain("PartId=");
            item.Link.ShouldContain("PartPlexId=");
            item.Link.ShouldContain("Quality=");
            item.Link.ShouldContain("LibraryId=");
            item.Link.ShouldContain("ServerId=");
        }
    }

    [Fact]
    public async Task ShouldReturnSpecificEpisode_WhenFilteredByImdbSeasonAndEpisode()
    {
        // Arrange
        await SetupDatabase(
            2002,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexTvShowLibraryCount = 1;
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 2;
            }
        );

        var episode = await IDbContext
            .PlexTvShowEpisodes.Include(e => e.TvShowSeason)
            .Include(e => e.TvShow)
            .OrderBy(e => e.Id)
            .FirstAsync(CancellationToken);

        var seasonNumber = episode.TvShowSeason!.SeasonNumber;
        var episodeNumber = episode.EpisodeNumber;
        var imdb = episode.TvShow!.Guid_IMDB!;

        var cmd = new SearchTvShowCommand
        {
            Query = string.Empty,
            Season = seasonNumber,
            Episode = episodeNumber,
            Limit = 100,
            Offset = 0,
            IMDB_ID = imdb.Replace("tt", ""),
            TMDB_ID = 0,
            TVDB_ID = 0,
        };

        // Act
        var result = await Sut.ExecuteAsync(cmd, CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Channel.Items.ShouldNotBeEmpty();
        // All returned items should correspond to the selected episode
        result
            .Channel.Items.All(i => i.Attributes.Any(a => a.Name == "season" && a.Value == seasonNumber.ToString()))
            .ShouldBeTrue();
        result
            .Channel.Items.All(i => i.Attributes.Any(a => a.Name == "episode" && a.Value == episodeNumber.ToString()))
            .ShouldBeTrue();
        result.Channel.Items.All(i => i.Attributes.Any(a => a.Name == "imdb" && a.Value == imdb)).ShouldBeTrue();

        // Titles should equal the part file name used during mapping
        var expectedTitle = await IDbContext
            .PlexTvShowEpisodeData.Where(d => d.PlexTvShowEpisodeId == episode.Id)
            .Select(d => d.GetFileName)
            .FirstAsync(CancellationToken);
        result.Channel.Items.Select(i => i.Title).Distinct().Single().ShouldBe(expectedTitle);

        // URLs should point to the torrent download endpoint
        result.Channel.Items.All(i => i.Link.Contains(PublicApiRoutes.DownloadTorrent)).ShouldBeTrue();

        // Database state (no mutations expected)
        var episodeExists = await IDbContext.PlexTvShowEpisodes.AnyAsync(e => e.Id == episode.Id, CancellationToken);
        episodeExists.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnSpecificEpisode_WhenFilteredByTmdbSeasonAndEpisode()
    {
        // Arrange
        await SetupDatabase(
            2103,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexTvShowLibraryCount = 1;
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 2;
            }
        );

        var episode = await IDbContext
            .PlexTvShowEpisodes.Include(e => e.TvShowSeason)
            .Include(e => e.TvShow)
            .OrderBy(e => e.Id)
            .FirstAsync(CancellationToken);

        var seasonNumber = episode.TvShowSeason!.SeasonNumber;
        var episodeNumber = episode.EpisodeNumber;
        var tmdb = episode.TvShow!.Guid_TMDB!.Value;

        var cmd = new SearchTvShowCommand
        {
            Query = string.Empty,
            Season = seasonNumber,
            Episode = episodeNumber,
            Limit = 100,
            Offset = 0,
            IMDB_ID = string.Empty,
            TMDB_ID = tmdb,
            TVDB_ID = 0,
        };

        // Act
        var result = await Sut.ExecuteAsync(cmd, CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Channel.Items.ShouldNotBeEmpty();
        result
            .Channel.Items.All(i => i.Attributes.Any(a => a.Name == "season" && a.Value == seasonNumber.ToString()))
            .ShouldBeTrue();
        result
            .Channel.Items.All(i => i.Attributes.Any(a => a.Name == "episode" && a.Value == episodeNumber.ToString()))
            .ShouldBeTrue();
        result
            .Channel.Items.All(i => i.Attributes.Any(a => a.Name == "tmdbid" && a.Value == tmdb.ToString()))
            .ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnSpecificEpisode_WhenFilteredByTvdbSeasonAndEpisode()
    {
        // Arrange
        await SetupDatabase(
            2204,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexTvShowLibraryCount = 1;
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 2;
            }
        );

        var episode = await IDbContext
            .PlexTvShowEpisodes.Include(e => e.TvShowSeason)
            .Include(e => e.TvShow)
            .OrderBy(e => e.Id)
            .FirstAsync(CancellationToken);

        var seasonNumber = episode.TvShowSeason!.SeasonNumber;
        var episodeNumber = episode.EpisodeNumber;
        var tvdb = episode.TvShow!.Guid_TVDB!.Value;

        var cmd = new SearchTvShowCommand
        {
            Query = string.Empty,
            Season = seasonNumber,
            Episode = episodeNumber,
            Limit = 100,
            Offset = 0,
            IMDB_ID = string.Empty,
            TMDB_ID = 0,
            TVDB_ID = tvdb,
        };

        // Act
        var result = await Sut.ExecuteAsync(cmd, CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Channel.Items.ShouldNotBeEmpty();
        result
            .Channel.Items.All(i => i.Attributes.Any(a => a.Name == "season" && a.Value == seasonNumber.ToString()))
            .ShouldBeTrue();
        result
            .Channel.Items.All(i => i.Attributes.Any(a => a.Name == "episode" && a.Value == episodeNumber.ToString()))
            .ShouldBeTrue();
        result
            .Channel.Items.All(i => i.Attributes.Any(a => a.Name == "tvdbid" && a.Value == tvdb.ToString()))
            .ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldSkipItems_WhenEpisodeMissingRelations()
    {
        // Arrange: empty DB first, then add an orphan episode with media
        await SetupDatabase(
            3003,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexTvShowLibraryCount = 1;
                config.TvShowCount = 0;
                config.TvShowSeasonCount = 0;
                config.TvShowEpisodeCount = 0;
            }
        );

        // Manually add an episode without TvShow/TvShowSeason
        var orphan = FakeData.GetPlexTvShowEpisode(new Seed(3003)).Generate();
        orphan.TvShow = null;
        orphan.TvShowSeason = null;
        await IDbContext.PlexTvShowEpisodes.AddAsync(orphan, CancellationToken);
        await IDbContext.SaveChangesAsync(CancellationToken);

        var initialItemCount = await IDbContext.PlexTvShowEpisodes.CountAsync(CancellationToken);

        var cmd = new SearchTvShowCommand
        {
            Query = string.Empty,
            Season = 0,
            Episode = 0,
            Limit = 50,
            Offset = 0,
            IMDB_ID = string.Empty,
            TMDB_ID = 0,
            TVDB_ID = 0,
        };

        // Act
        var result = await Sut.ExecuteAsync(cmd, CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        // Orphan should be skipped resulting in 0 items since DB has only the orphan
        result.Channel.Items.ShouldBeEmpty();

        // Database state unchanged in counts
        var finalCount = await IDbContext.PlexTvShowEpisodes.CountAsync(CancellationToken);
        finalCount.ShouldBe(initialItemCount);
    }

    [Fact]
    public async Task ShouldSkipItem_WhenEpisodeMissingOnlyTvShow()
    {
        // Arrange
        await SetupDatabase(
            3004,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexTvShowLibraryCount = 1;
            }
        );

        var ep = FakeData.GetPlexTvShowEpisode(new Seed(3004)).Generate();
        ep.TvShow = null;
        // Keep season so only TvShow missing
        await IDbContext.PlexTvShowEpisodes.AddAsync(ep, CancellationToken);
        await IDbContext.SaveChangesAsync(CancellationToken);

        var result = await Sut.ExecuteAsync(
            new SearchTvShowCommand
            {
                Query = string.Empty,
                Season = 0,
                Episode = 0,
                Limit = 10,
                Offset = 0,
                IMDB_ID = string.Empty,
                TMDB_ID = 0,
                TVDB_ID = 0,
            },
            CancellationToken
        );

        result.Channel.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldSkipItem_WhenEpisodeMissingOnlySeason()
    {
        // Arrange
        await SetupDatabase(
            3005,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexTvShowLibraryCount = 1;
            }
        );

        var ep = FakeData.GetPlexTvShowEpisode(new Seed(3005)).Generate();
        ep.TvShowSeason = null;
        await IDbContext.PlexTvShowEpisodes.AddAsync(ep, CancellationToken);
        await IDbContext.SaveChangesAsync(CancellationToken);

        var result = await Sut.ExecuteAsync(
            new SearchTvShowCommand
            {
                Query = string.Empty,
                Season = 0,
                Episode = 0,
                Limit = 10,
                Offset = 0,
                IMDB_ID = string.Empty,
                TMDB_ID = 0,
                TVDB_ID = 0,
            },
            CancellationToken
        );

        result.Channel.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldCreateMultipleItemsPerEpisode_WhenMultiPartEpisodesEnabled()
    {
        // Arrange
        await SetupDatabase(
            3106,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexTvShowLibraryCount = 1;
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 3;
                config.IncludeMultiPartEpisodes = true;
            }
        );

        var offset = 0;
        var limit = 2;
        var cmd = new SearchTvShowCommand
        {
            Query = string.Empty,
            Season = 0,
            Episode = 0,
            Limit = limit,
            Offset = offset,
            IMDB_ID = string.Empty,
            TMDB_ID = 0,
            TVDB_ID = 0,
        };

        // Expected total parts across the paged episodes
        var expectedPartCount = await IDbContext
            .PlexTvShowEpisodes.Include(e => e.MediaDataList)
            .OrderBy(e => e.Id)
            .Skip(offset)
            .Take(limit)
            .Select(e => e.MediaDataList.Count)
            .SumAsync(CancellationToken);

        // Act
        var result = await Sut.ExecuteAsync(cmd, CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Channel.Items.Count.ShouldBe(expectedPartCount);
    }

    [Fact]
    public async Task ShouldReturnEmpty_WhenQueryProvidedWithoutSeasonEpisode()
    {
        // Arrange
        await SetupDatabase(
            3207,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexTvShowLibraryCount = 1;
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 2;
            }
        );

        var cmd = new SearchTvShowCommand
        {
            Query = "anything",
            Season = 0,
            Episode = 0,
            Limit = 10,
            Offset = 0,
            IMDB_ID = string.Empty,
            TMDB_ID = 0,
            TVDB_ID = 0,
        };

        // Act
        var result = await Sut.ExecuteAsync(cmd, CancellationToken);

        // Assert
        result.Channel.Items.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldValidate_WhenPagingOnlyProvided()
    {
        // Arrange
        var validator = new SearchTvShowCommandValidator();
        var cmd = new SearchTvShowCommand
        {
            Query = string.Empty,
            Season = 0,
            Episode = 0,
            Limit = 10,
            Offset = 0,
            IMDB_ID = string.Empty,
            TMDB_ID = 0,
            TVDB_ID = 0,
        };

        // Act
        var result = validator.Validate(cmd);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldFailValidation_WhenSeasonAndEpisodeProvidedWithoutExternalIds()
    {
        // Arrange
        var validator = new SearchTvShowCommandValidator();
        var cmd = new SearchTvShowCommand
        {
            Query = string.Empty,
            Season = 1,
            Episode = 1,
            Limit = 10,
            Offset = 0,
            IMDB_ID = string.Empty,
            TMDB_ID = 0,
            TVDB_ID = 0,
        };

        // Act
        var result = validator.Validate(cmd);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public void ShouldPassValidation_WhenSeasonEpisodeWithImdbProvided()
    {
        // Arrange
        var validator = new SearchTvShowCommandValidator();
        var cmd = new SearchTvShowCommand
        {
            Query = string.Empty,
            Season = 1,
            Episode = 2,
            Limit = 10,
            Offset = 0,
            IMDB_ID = "imdb://tt12345",
            TMDB_ID = 0,
            TVDB_ID = 0,
        };

        // Act
        var result = validator.Validate(cmd);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldFailValidation_WhenLimitIsZero()
    {
        // Arrange
        var validator = new SearchTvShowCommandValidator();
        var cmd = new SearchTvShowCommand
        {
            Query = string.Empty,
            Season = 0,
            Episode = 0,
            Limit = 0,
            Offset = 0,
            IMDB_ID = string.Empty,
            TMDB_ID = 0,
            TVDB_ID = 0,
        };

        // Act
        var result = validator.Validate(cmd);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public void ShouldFailValidation_WhenLimitExceedsMax()
    {
        // Arrange
        var validator = new SearchTvShowCommandValidator();
        var cmd = new SearchTvShowCommand
        {
            Query = string.Empty,
            Season = 0,
            Episode = 0,
            Limit = 501,
            Offset = 0,
            IMDB_ID = string.Empty,
            TMDB_ID = 0,
            TVDB_ID = 0,
        };

        // Act
        var result = validator.Validate(cmd);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public void ShouldFailValidation_WhenOffsetIsNegative()
    {
        // Arrange
        var validator = new SearchTvShowCommandValidator();
        var cmd = new SearchTvShowCommand
        {
            Query = string.Empty,
            Season = 0,
            Episode = 0,
            Limit = 10,
            Offset = -1,
            IMDB_ID = string.Empty,
            TMDB_ID = 0,
            TVDB_ID = 0,
        };

        // Act
        var result = validator.Validate(cmd);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public void ShouldFailValidation_WhenSeasonIsNegative()
    {
        // Arrange
        var validator = new SearchTvShowCommandValidator();
        var cmd = new SearchTvShowCommand
        {
            Query = string.Empty,
            Season = -1,
            Episode = 0,
            Limit = 10,
            Offset = 0,
            IMDB_ID = string.Empty,
            TMDB_ID = 0,
            TVDB_ID = 0,
        };

        // Act
        var result = validator.Validate(cmd);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public void ShouldFailValidation_WhenEpisodeIsNegative()
    {
        // Arrange
        var validator = new SearchTvShowCommandValidator();
        var cmd = new SearchTvShowCommand
        {
            Query = string.Empty,
            Season = 0,
            Episode = -1,
            Limit = 10,
            Offset = 0,
            IMDB_ID = string.Empty,
            TMDB_ID = 0,
            TVDB_ID = 0,
        };

        // Act
        var result = validator.Validate(cmd);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public void ShouldFailValidation_WhenTmdbIdIsNegative()
    {
        // Arrange
        var validator = new SearchTvShowCommandValidator();
        var cmd = new SearchTvShowCommand
        {
            Query = string.Empty,
            Season = 0,
            Episode = 0,
            Limit = 10,
            Offset = 0,
            IMDB_ID = string.Empty,
            TMDB_ID = -1,
            TVDB_ID = 0,
        };

        // Act
        var result = validator.Validate(cmd);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public void ShouldFailValidation_WhenTvdbIdIsNegative()
    {
        // Arrange
        var validator = new SearchTvShowCommandValidator();
        var cmd = new SearchTvShowCommand
        {
            Query = string.Empty,
            Season = 0,
            Episode = 0,
            Limit = 10,
            Offset = 0,
            IMDB_ID = string.Empty,
            TMDB_ID = 0,
            TVDB_ID = -1,
        };

        // Act
        var result = validator.Validate(cmd);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task ShouldReturnSpecificEpisode_WhenAllExternalIdsProvided()
    {
        // Arrange
        await SetupDatabase(
            3308,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexTvShowLibraryCount = 1;
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 2;
            }
        );

        var episode = await IDbContext
            .PlexTvShowEpisodes.Include(e => e.TvShowSeason)
            .Include(e => e.TvShow)
            .OrderBy(e => e.Id)
            .FirstAsync(CancellationToken);

        var seasonNumber = episode.TvShowSeason!.SeasonNumber;
        var episodeNumber = episode.EpisodeNumber;
        var imdb = episode.TvShow!.Guid_IMDB!;
        var tmdb = episode.TvShow!.Guid_TMDB!.Value;
        var tvdb = episode.TvShow!.Guid_TVDB!.Value;

        var cmd = new SearchTvShowCommand
        {
            Query = string.Empty,
            Season = seasonNumber,
            Episode = episodeNumber,
            Limit = 100,
            Offset = 0,
            IMDB_ID = imdb.Replace("tt", ""),
            TMDB_ID = tmdb,
            TVDB_ID = tvdb,
        };

        // Act
        var result = await Sut.ExecuteAsync(cmd, CancellationToken);

        // Assert
        result.Channel.Items.ShouldNotBeEmpty();
        result.Channel.Items.All(i => i.Attributes.Any(a => a.Name == "imdb" && a.Value == imdb)).ShouldBeTrue();
        result
            .Channel.Items.All(i => i.Attributes.Any(a => a.Name == "tmdbid" && a.Value == tmdb.ToString()))
            .ShouldBeTrue();
        result
            .Channel.Items.All(i => i.Attributes.Any(a => a.Name == "tvdbid" && a.Value == tvdb.ToString()))
            .ShouldBeTrue();
    }
}
