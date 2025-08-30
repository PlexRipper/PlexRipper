using LukeHagar.PlexAPI.SDK.Models.Requests;
using Reaparr.BaseTests;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.PlexApi.UnitTests;

public class PlexMediaTypeMappersToGetAllLibrariesTypeUnitTests : BaseUnitTest
{
    public PlexMediaTypeMappersToGetAllLibrariesTypeUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Theory]
    [InlineData(PlexMediaType.Movie, GetAllLibrariesType.Movie)]
    [InlineData(PlexMediaType.TvShow, GetAllLibrariesType.TvShow)]
    [InlineData(PlexMediaType.Season, GetAllLibrariesType.Season)]
    [InlineData(PlexMediaType.Episode, GetAllLibrariesType.Episode)]
    [InlineData(PlexMediaType.Artist, GetAllLibrariesType.Artist)]
    [InlineData(PlexMediaType.Album, GetAllLibrariesType.Album)]
    [InlineData(PlexMediaType.Song, GetAllLibrariesType.Track)]
    [InlineData(PlexMediaType.PhotoAlbum, GetAllLibrariesType.PhotoAlbum)]
    [InlineData(PlexMediaType.Photos, GetAllLibrariesType.Photo)]
    public void ShouldConvertFromPlexMediaTypeToGetAllLibrariesType_WhenGivenAPlexMediaTypeValue(
        PlexMediaType input,
        GetAllLibrariesType expected
    )
    {
        // Act
        input.ToGetAllLibrariesType().ShouldBe(expected);
    }

    [Theory]
    [InlineData(GetAllLibrariesType.Movie, PlexMediaType.Movie)]
    [InlineData(GetAllLibrariesType.TvShow, PlexMediaType.TvShow)]
    [InlineData(GetAllLibrariesType.Season, PlexMediaType.Season)]
    [InlineData(GetAllLibrariesType.Episode, PlexMediaType.Episode)]
    [InlineData(GetAllLibrariesType.Artist, PlexMediaType.Artist)]
    [InlineData(GetAllLibrariesType.Album, PlexMediaType.Album)]
    [InlineData(GetAllLibrariesType.Track, PlexMediaType.Song)]
    [InlineData(GetAllLibrariesType.PhotoAlbum, PlexMediaType.PhotoAlbum)]
    [InlineData(GetAllLibrariesType.Photo, PlexMediaType.Photos)]
    public void ShouldConvertFromGetAllLibrariesTypeToPlexMediaType_WhenGivenAGetAllLibrariesTypeValue(
        GetAllLibrariesType input,
        PlexMediaType expected
    )
    {
        // Act
        input.ToPlexMediaType().ShouldBe(expected);
    }
}
