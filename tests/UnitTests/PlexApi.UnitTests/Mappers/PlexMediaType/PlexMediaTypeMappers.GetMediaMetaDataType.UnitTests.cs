using LukeHagar.PlexAPI.SDK.Models.Requests;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.PlexApi.UnitTests;

public class PlexMediaTypeMappersToGetMediaMetaDataTypeUnitTests : BaseUnitTest
{
    public PlexMediaTypeMappersToGetMediaMetaDataTypeUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Theory]
    [InlineData(PlexMediaType.Movie, GetMediaMetaDataType.Movie)]
    [InlineData(PlexMediaType.TvShow, GetMediaMetaDataType.TvShow)]
    [InlineData(PlexMediaType.Season, GetMediaMetaDataType.Season)]
    [InlineData(PlexMediaType.Episode, GetMediaMetaDataType.Episode)]
    [InlineData(PlexMediaType.Artist, GetMediaMetaDataType.Artist)]
    [InlineData(PlexMediaType.Album, GetMediaMetaDataType.Album)]
    [InlineData(PlexMediaType.Song, GetMediaMetaDataType.Track)]
    [InlineData(PlexMediaType.PhotoAlbum, GetMediaMetaDataType.PhotoAlbum)]
    [InlineData(PlexMediaType.Photos, GetMediaMetaDataType.Photo)]
    public void ShouldConvertFromPlexMediaTypeToGetMediaMetaDataType_WhenGivenAPlexMediaTypeValue(
        PlexMediaType input,
        GetMediaMetaDataType expected
    )
    {
        // Act
        input.ToGetMediaMetaDataType().ShouldBe(expected);
    }

    [Theory]
    [InlineData(GetMediaMetaDataType.Movie, PlexMediaType.Movie)]
    [InlineData(GetMediaMetaDataType.TvShow, PlexMediaType.TvShow)]
    [InlineData(GetMediaMetaDataType.Season, PlexMediaType.Season)]
    [InlineData(GetMediaMetaDataType.Episode, PlexMediaType.Episode)]
    [InlineData(GetMediaMetaDataType.Artist, PlexMediaType.Artist)]
    [InlineData(GetMediaMetaDataType.Album, PlexMediaType.Album)]
    [InlineData(GetMediaMetaDataType.Track, PlexMediaType.Song)]
    [InlineData(GetMediaMetaDataType.PhotoAlbum, PlexMediaType.PhotoAlbum)]
    [InlineData(GetMediaMetaDataType.Photo, PlexMediaType.Photos)]
    public void ShouldConvertFromGetMediaMetaDataTypeToPlexMediaType_WhenGivenAGetMediaMetaDataTypeValue(
        GetMediaMetaDataType input,
        PlexMediaType expected
    )
    {
        // Act
        input.ToPlexMediaType().ShouldBe(expected);
    }
}
