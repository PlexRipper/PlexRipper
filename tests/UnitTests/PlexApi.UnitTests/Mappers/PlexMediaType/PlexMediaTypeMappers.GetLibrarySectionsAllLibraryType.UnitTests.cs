using LukeHagar.PlexAPI.SDK.Models.Requests;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.PlexApi.UnitTests;

public class PlexMediaTypeMappersToGetLibrarySectionsAllLibraryTypeUnitTests : BaseUnitTest
{
    public PlexMediaTypeMappersToGetLibrarySectionsAllLibraryTypeUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Theory]
    [InlineData(PlexMediaType.Movie, GetLibrarySectionsAllLibraryType.Movie)]
    [InlineData(PlexMediaType.TvShow, GetLibrarySectionsAllLibraryType.TvShow)]
    [InlineData(PlexMediaType.Season, GetLibrarySectionsAllLibraryType.Season)]
    [InlineData(PlexMediaType.Episode, GetLibrarySectionsAllLibraryType.Episode)]
    [InlineData(PlexMediaType.Artist, GetLibrarySectionsAllLibraryType.Artist)]
    [InlineData(PlexMediaType.Album, GetLibrarySectionsAllLibraryType.Album)]
    [InlineData(PlexMediaType.Song, GetLibrarySectionsAllLibraryType.Track)]
    [InlineData(PlexMediaType.PhotoAlbum, GetLibrarySectionsAllLibraryType.PhotoAlbum)]
    [InlineData(PlexMediaType.Photos, GetLibrarySectionsAllLibraryType.Photo)]
    public void ShouldConvertFromPlexMediaTypeToGetLibrarySectionsAllLibraryType_WhenGivenAPlexMediaTypeValue(
        PlexMediaType input,
        GetLibrarySectionsAllLibraryType expected
    )
    {
        // Act
        input.ToGetLibrarySectionsAllLibraryType().ShouldBe(expected);
    }

    [Theory]
    [InlineData(GetLibrarySectionsAllLibraryType.Movie, PlexMediaType.Movie)]
    [InlineData(GetLibrarySectionsAllLibraryType.TvShow, PlexMediaType.TvShow)]
    [InlineData(GetLibrarySectionsAllLibraryType.Season, PlexMediaType.Season)]
    [InlineData(GetLibrarySectionsAllLibraryType.Episode, PlexMediaType.Episode)]
    [InlineData(GetLibrarySectionsAllLibraryType.Artist, PlexMediaType.Artist)]
    [InlineData(GetLibrarySectionsAllLibraryType.Album, PlexMediaType.Album)]
    [InlineData(GetLibrarySectionsAllLibraryType.Track, PlexMediaType.Song)]
    [InlineData(GetLibrarySectionsAllLibraryType.PhotoAlbum, PlexMediaType.PhotoAlbum)]
    [InlineData(GetLibrarySectionsAllLibraryType.Photo, PlexMediaType.Photos)]
    public void ShouldConvertFromGetLibrarySectionsAllLibraryTypeToPlexMediaType_WhenGivenAGetLibrarySectionsAllLibraryTypeValue(
        GetLibrarySectionsAllLibraryType input,
        PlexMediaType expected
    )
    {
        // Act
        input.ToPlexMediaType().ShouldBe(expected);
    }
}
