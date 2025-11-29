using LukeHagar.PlexAPI.SDK.Models.Components;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.PlexApi.UnitTests;

public class PlexMediaTypeMappersToGetLibrarySectionsAllQueryParamTypeUnitTests : BaseUnitTest
{
    public PlexMediaTypeMappersToGetLibrarySectionsAllQueryParamTypeUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Theory]
    [InlineData(PlexMediaType.Movie, MediaType.Movie)]
    [InlineData(PlexMediaType.TvShow, MediaType.TvShow)]
    [InlineData(PlexMediaType.Season, MediaType.Season)]
    [InlineData(PlexMediaType.Episode, MediaType.Episode)]
    [InlineData(PlexMediaType.Artist, MediaType.Artist)]
    [InlineData(PlexMediaType.Album, MediaType.Album)]
    [InlineData(PlexMediaType.Song, MediaType.Track)]
    [InlineData(PlexMediaType.PhotoAlbum, MediaType.PhotoAlbum)]
    [InlineData(PlexMediaType.Photos, MediaType.Photo)]
    public void ShouldMatchTheCorrectPlexMediaType_WhenGivenAGetLibrarySectionsAllQueryParamTypeValue(
        PlexMediaType input,
        MediaType expected
    )
    {
        // Act
        input.ToPlexApiMediaType().ShouldBe(expected);
    }
}
