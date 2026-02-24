using Microsoft.EntityFrameworkCore;
using Reaparr.Settings.Contracts;

namespace Reaparr.PublicAPI.UnitTests;

public class SearchMovieCommandUnitTests : BaseUnitTest<SearchMovieCommandHandler>
{
    public SearchMovieCommandUnitTests(ITestOutputHelper output)
        : base(output)
    {
        Mock.Mock<INetworkSettings>().SetupGet(x => x.Url).Returns("http://localhost");
    }

    [Fact]
    public async Task ShouldReturnPagedMovies_WhenNoFiltersProvided()
    {
        // Arrange
        await SetupDatabase(
            4001,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 5;
                config.IncludeMultiPartMovies = false;
            }
        );

        var offset = 1;
        var limit = 3;
        var cmd = new SearchMovieCommand
        {
            Query = string.Empty,
            Limit = limit,
            Offset = offset,
            IMDB_ID = string.Empty,
            TMDB_ID = 0,
        };

        var movies = await IDbContext
            .PlexMovies.AsNoTracking()
            .Include(m => m.MediaDataList)
            .OrderBy(m => m.Id)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(CancellationToken);

        var expectedMovieTitles = movies
            .SelectMany(m => m.MediaDataList.OrderBy(md => md.PlexApiPartId).Select(md => md.GetFileName))
            .ToList();

        // Act
        var result = await Sut.ExecuteAsync(cmd, CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Channel.ShouldNotBeNull();
        result.Channel.Title.ShouldBe("Reaparr Indexer");
        result.Channel.Description.ShouldBe($"Movie Search results for {cmd.Query}");
        result.Channel.Language.ShouldBe("en-us");
        result.Channel.Category.ShouldBe("search");

        result.Channel.Items.ShouldNotBeNull();
        result.Channel.Items.Count.ShouldBe(expectedMovieTitles.Count);
        result.Channel.Items.Select(i => i.Title).ToList().ShouldBe(expectedMovieTitles);

        foreach (var item in result.Channel.Items)
        {
            item.Guid.ShouldNotBeNull();
            item.Guid.IsPermaLink.ShouldBe("false");
            item.Guid.Value.ShouldBe(item.Link);

            item.Enclosure.ShouldNotBeNull();
            item.Enclosure.Type.ShouldBe("application/x-bittorrent");
            item.Enclosure.Length.ShouldBe(item.Size);

            item.Attributes.Any(a => a.Name == "type" && a.Value == "movie").ShouldBeTrue();
            item.Attributes.Any(a => a.Name == "language" && a.Value == "English").ShouldBeTrue();
            item.Attributes.Any(a => a.Name == "downloadvolumefactor" && a.Value == "0.0").ShouldBeTrue();
            item.Attributes.Any(a => a.Name == "seeders" && int.Parse(a.Value) > 0).ShouldBeTrue();
            item.Attributes.Any(a => a.Name == "peers" && int.Parse(a.Value) > 0).ShouldBeTrue();

            item.Attributes.Any(a => a.Name == "category").ShouldBeTrue();
            item.Attributes.Any(a => a.Name == "resolution").ShouldBeTrue();
            item.Attributes.Any(a => a.Name == "source").ShouldBeTrue();
            item.Attributes.Any(a => a.Name == "videoCodec").ShouldBeTrue();
            item.Attributes.Any(a => a.Name == "audioCodec").ShouldBeTrue();

            item.Link.ShouldContain(PublicApiRoutes.DownloadTorrent);
            item.Link.ShouldContain("Type=Movie");
            item.Link.ShouldContain("MediaId=");
            item.Link.ShouldContain("DataId=");
            item.Link.ShouldContain("PartId=");
            item.Link.ShouldContain("PlexApiPartId=");
            item.Link.ShouldContain("Quality=");
            item.Link.ShouldContain("LibraryId=");
            item.Link.ShouldContain("ServerId=");
        }
    }

    [Fact]
    public async Task ShouldReturnSpecificMovie_WhenFilteredByImdbId()
    {
        // Arrange
        await SetupDatabase(
            4102,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 2;
            }
        );

        var dbContext = IDbContext;
        var movie = await dbContext
            .PlexMovies.Include(m => m.MediaDataList)
            .OrderBy(m => m.Id)
            .FirstAsync(m => !string.IsNullOrEmpty(m.Guid_IMDB), CancellationToken);

        var imdb = movie.Guid_IMDB!;

        var cmd = new SearchMovieCommand
        {
            Query = string.Empty,
            Limit = 100,
            Offset = 0,
            IMDB_ID = imdb.Replace("tt", string.Empty),
            TMDB_ID = 0,
        };

        // Act
        var result = await Sut.ExecuteAsync(cmd, CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Channel.Items.ShouldNotBeEmpty();
        result.Channel.Items.All(i => i.Attributes.Any(a => a.Name == "imdb" && a.Value == imdb)).ShouldBeTrue();

        var expectedTitles = movie.MediaDataList.OrderBy(md => md.PlexApiPartId).Select(md => md.GetFileName).ToList();
        result.Channel.Items.Select(i => i.Title).ToList().ShouldBe(expectedTitles);
        result.Channel.Items.All(i => i.Link.Contains(PublicApiRoutes.DownloadTorrent)).ShouldBeTrue();

        var movieExists = await dbContext.PlexMovies.AnyAsync(m => m.Id == movie.Id, CancellationToken);
        movieExists.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnSpecificMovie_WhenFilteredByTmdbId()
    {
        // Arrange
        await SetupDatabase(
            4203,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 2;
            }
        );

        var movie = await IDbContext
            .PlexMovies.Include(m => m.MediaDataList)
            .OrderBy(m => m.Id)
            .FirstAsync(m => m.Guid_TMDB.HasValue, CancellationToken);

        var tmdb = movie.Guid_TMDB!.Value;

        var cmd = new SearchMovieCommand
        {
            Query = string.Empty,
            Limit = 100,
            Offset = 0,
            IMDB_ID = string.Empty,
            TMDB_ID = tmdb,
        };

        // Act
        var result = await Sut.ExecuteAsync(cmd, CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Channel.Items.ShouldNotBeEmpty();
        result
            .Channel.Items.All(i => i.Attributes.Any(a => a.Name == "tmdbid" && a.Value == tmdb.ToString()))
            .ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnEmpty_WhenNoMoviesExist()
    {
        // Arrange
        await SetupDatabase(
            4304,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 0;
            }
        );

        var cmd = new SearchMovieCommand
        {
            Query = string.Empty,
            Limit = 50,
            Offset = 0,
            IMDB_ID = string.Empty,
            TMDB_ID = 0,
        };

        // Act
        var result = await Sut.ExecuteAsync(cmd, CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Channel.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldCreateMultipleItemsPerMovie_WhenMultiPartMoviesEnabled()
    {
        // Arrange
        await SetupDatabase(
            4405,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 3;
                config.IncludeMultiPartMovies = true;
            }
        );

        var offset = 0;
        var limit = 2;
        var cmd = new SearchMovieCommand
        {
            Query = string.Empty,
            Limit = limit,
            Offset = offset,
            IMDB_ID = string.Empty,
            TMDB_ID = 0,
        };

        var expectedPartCount = await IDbContext
            .PlexMovies.Include(m => m.MediaDataList)
            .OrderBy(m => m.Id)
            .Skip(offset)
            .Take(limit)
            .Select(m => m.MediaDataList.Count)
            .SumAsync(CancellationToken);

        // Act
        var result = await Sut.ExecuteAsync(cmd, CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Channel.Items.Count.ShouldBe(expectedPartCount);
    }

    [Fact]
    public async Task ShouldReturnEmpty_WhenQueryNormalizesToEmpty()
    {
        // Arrange
        await SetupDatabase(
            4506,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 2;
            }
        );

        var cmd = new SearchMovieCommand
        {
            Query = "!!!",
            Limit = 10,
            Offset = 0,
            IMDB_ID = string.Empty,
            TMDB_ID = 0,
        };

        // Act
        var result = await Sut.ExecuteAsync(cmd, CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Channel.Items.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldValidate_WhenPagingOnlyProvided()
    {
        // Arrange
        var validator = new SearchMovieCommandValidator();
        var cmd = new SearchMovieCommand
        {
            Query = string.Empty,
            Limit = 10,
            Offset = 0,
            IMDB_ID = string.Empty,
            TMDB_ID = 0,
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
        var validator = new SearchMovieCommandValidator();
        var cmd = new SearchMovieCommand
        {
            Query = string.Empty,
            Limit = 0,
            Offset = 0,
            IMDB_ID = string.Empty,
            TMDB_ID = 0,
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
        var validator = new SearchMovieCommandValidator();
        var cmd = new SearchMovieCommand
        {
            Query = string.Empty,
            Limit = 501,
            Offset = 0,
            IMDB_ID = string.Empty,
            TMDB_ID = 0,
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
        var validator = new SearchMovieCommandValidator();
        var cmd = new SearchMovieCommand
        {
            Query = string.Empty,
            Limit = 10,
            Offset = -1,
            IMDB_ID = string.Empty,
            TMDB_ID = 0,
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
        var validator = new SearchMovieCommandValidator();
        var cmd = new SearchMovieCommand
        {
            Query = string.Empty,
            Limit = 10,
            Offset = 0,
            IMDB_ID = string.Empty,
            TMDB_ID = -1,
        };

        // Act
        var result = validator.Validate(cmd);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public void ShouldFailValidation_WhenImdbIdIsNull()
    {
        // Arrange
        var validator = new SearchMovieCommandValidator();
        var cmd = new SearchMovieCommand
        {
            Query = string.Empty,
            Limit = 10,
            Offset = 0,
            IMDB_ID = null!,
            TMDB_ID = 0,
        };

        // Act
        var result = validator.Validate(cmd);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }
}
