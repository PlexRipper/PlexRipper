using Reaparr.Data.Contracts;

namespace Reaparr.Data.UnitTests;

public class DbContextExtensionsPlexMediaUnitTests : BaseUnitTest
{
    public DbContextExtensionsPlexMediaUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldFindMovieId_ByMediaKey()
    {
        // Arrange
        await SetupDatabase(
            12400,
            cfg =>
            {
                cfg.PlexServerCount = 1;
                cfg.PlexMovieLibraryCount = 1;
                cfg.MovieCount = 5;
            }
        );
        var movie = IDbContext.PlexMovies.First();

        // Act
        var result = await IDbContext.GetPlexMediaByMediaKeyAsync(
            movie.Key,
            movie.PlexServerId,
            PlexMediaType.Movie,
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(movie.Id);
    }

    [Fact]
    public async Task ShouldFindTvShowId_ByMediaKey()
    {
        // Arrange
        await SetupDatabase(
            12401,
            cfg =>
            {
                cfg.PlexServerCount = 1;
                cfg.PlexTvShowLibraryCount = 1;
                cfg.TvShowCount = 5;
            }
        );
        var tvShow = IDbContext.PlexTvShows.First();

        // Act
        var result = await IDbContext.GetPlexMediaByMediaKeyAsync(
            tvShow.Key,
            tvShow.PlexServerId,
            PlexMediaType.TvShow,
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(tvShow.Id);
    }

    [Fact]
    public async Task ShouldFail_WhenMediaNotFound()
    {
        // Arrange
        await SetupDatabase(
            12402,
            cfg =>
            {
                cfg.PlexServerCount = 1;
                cfg.PlexMovieLibraryCount = 1;
            }
        );

        // Act
        var result = await IDbContext.GetPlexMediaByMediaKeyAsync(9999, 1, PlexMediaType.Movie, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }
}
