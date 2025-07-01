using PlexApi.Contracts;

namespace PlexApi.UnitTests;

public class PlexMediaTypeApiStringMapperUnitTests : BaseUnitTest
{
    public PlexMediaTypeApiStringMapperUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Theory]
    [InlineData(PlexMediaType.Movie, "movie")]
    [InlineData(PlexMediaType.TvShow, "show")]
    [InlineData(PlexMediaType.Season, "season")]
    [InlineData(PlexMediaType.Episode, "episode")]
    [InlineData(PlexMediaType.Artist, "artist")]
    [InlineData(PlexMediaType.Album, "album")]
    [InlineData(PlexMediaType.Song, "track")]
    [InlineData(PlexMediaType.PhotoAlbum, "photoalbum")]
    [InlineData(PlexMediaType.Photos, "photo")]
    public void ShouldConvertFromPlexMediaTypeToApiString_WhenGivenValidPlexMediaType(
        PlexMediaType input,
        string expected
    )
    {
        // Act & Assert
        input.ToPlexApiString().ShouldBe(expected);
    }

    [Theory]
    [InlineData("movie", PlexMediaType.Movie)]
    [InlineData("show", PlexMediaType.TvShow)]
    [InlineData("season", PlexMediaType.Season)]
    [InlineData("episode", PlexMediaType.Episode)]
    [InlineData("artist", PlexMediaType.Artist)]
    [InlineData("album", PlexMediaType.Album)]
    [InlineData("track", PlexMediaType.Song)]
    [InlineData("photoalbum", PlexMediaType.PhotoAlbum)]
    [InlineData("photo", PlexMediaType.Photos)]
    [InlineData("MOVIE", PlexMediaType.Movie)] // casing test
    [InlineData("Track", PlexMediaType.Song)] // casing test
    public void ShouldConvertFromApiStringToPlexMediaType_WhenGivenValidApiString(string input, PlexMediaType expected)
    {
        // Act & Assert
        input.ToPlexMediaTypeFromPlexApi().ShouldBe(expected);
    }

    [Fact]
    public void ShouldThrowException_WhenConvertingUnknownStringToPlexMediaType()
    {
        // Arrange
        const string unknown = "invalid_type";

        // Act & Assert
        Should.Throw<NotImplementedException>(() =>
        {
            _ = unknown.ToPlexMediaTypeFromPlexApi();
        });
    }

    [Fact]
    public void ShouldThrowException_WhenConvertingUnknownPlexMediaTypeToApiString()
    {
        // Arrange
        const PlexMediaType unknown = PlexMediaType.Music;

        // Act & Assert
        Should.Throw<NotImplementedException>(() =>
        {
            _ = unknown.ToPlexApiString();
        });
    }
}
