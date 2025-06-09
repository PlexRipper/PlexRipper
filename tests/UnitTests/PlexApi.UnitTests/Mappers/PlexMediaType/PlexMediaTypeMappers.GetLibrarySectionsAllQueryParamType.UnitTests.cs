using LukeHagar.PlexAPI.SDK.Models.Requests;
using PlexApi.Contracts;

namespace PlexApi.UnitTests;

public class PlexMediaTypeMappersToGetLibrarySectionsAllQueryParamTypeUnitTests : BaseUnitTest
{
    public PlexMediaTypeMappersToGetLibrarySectionsAllQueryParamTypeUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Theory]
    [InlineData(PlexMediaType.Movie, GetLibrarySectionsAllQueryParamType.Movie)]
    [InlineData(PlexMediaType.TvShow, GetLibrarySectionsAllQueryParamType.TvShow)]
    [InlineData(PlexMediaType.Season, GetLibrarySectionsAllQueryParamType.Season)]
    [InlineData(PlexMediaType.Episode, GetLibrarySectionsAllQueryParamType.Episode)]
    [InlineData(PlexMediaType.Artist, GetLibrarySectionsAllQueryParamType.Artist)]
    [InlineData(PlexMediaType.Album, GetLibrarySectionsAllQueryParamType.Album)]
    [InlineData(PlexMediaType.Song, GetLibrarySectionsAllQueryParamType.Track)]
    [InlineData(PlexMediaType.PhotoAlbum, GetLibrarySectionsAllQueryParamType.PhotoAlbum)]
    [InlineData(PlexMediaType.Photos, GetLibrarySectionsAllQueryParamType.Photo)]
    public void ShouldMatchTheCorrectPlexMediaType_WhenGivenAGetLibrarySectionsAllQueryParamTypeValue(
        PlexMediaType input,
        GetLibrarySectionsAllQueryParamType expected
    )
    {
        // Act
        input.ToGetLibrarySectionsAllQueryParamType().ShouldBe(expected);
    }
}
