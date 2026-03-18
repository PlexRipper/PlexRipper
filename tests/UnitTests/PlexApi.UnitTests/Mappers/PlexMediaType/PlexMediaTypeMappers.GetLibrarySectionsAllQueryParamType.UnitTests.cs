using LukeHagar.PlexAPI.SDK.Models.Components;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.PlexApi.UnitTests;

public class PlexMediaTypeMappersToPlexApiMediaTypeUnitTests : BaseUnitTest
{
    public PlexMediaTypeMappersToPlexApiMediaTypeUnitTests()
        : base() { }

    [Test]
    [Arguments(PlexMediaType.Movie, MediaType.Movie)]
    [Arguments(PlexMediaType.TvShow, MediaType.TvShow)]
    [Arguments(PlexMediaType.Season, MediaType.Season)]
    [Arguments(PlexMediaType.Episode, MediaType.Episode)]
    [Arguments(PlexMediaType.Artist, MediaType.Artist)]
    [Arguments(PlexMediaType.Album, MediaType.Album)]
    [Arguments(PlexMediaType.Song, MediaType.Track)]
    [Arguments(PlexMediaType.PhotoAlbum, MediaType.PhotoAlbum)]
    [Arguments(PlexMediaType.Photos, MediaType.Photo)]
    public void ShouldMapPlexMediaTypeToMediaType(PlexMediaType input, MediaType expected)
    {
        // Act
        input.ToPlexApiMediaType().ShouldBe(expected);
    }
}
