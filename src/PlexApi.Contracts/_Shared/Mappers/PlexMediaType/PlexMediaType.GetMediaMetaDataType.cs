using LukeHagar.PlexAPI.SDK.Models.Requests;

namespace Reaparr.PlexApi.Contracts;

public static partial class PlexMediaTypeMappers
{
    public static GetMediaMetaDataType ToGetMediaMetaDataType(this PlexMediaType source) =>
        source.ToApiTypeEnum<GetMediaMetaDataType>();

    public static PlexMediaType ToPlexMediaType(this GetMediaMetaDataType source) =>
        source switch
        {
            GetMediaMetaDataType.Movie => PlexMediaType.Movie,
            GetMediaMetaDataType.TvShow => PlexMediaType.TvShow,
            GetMediaMetaDataType.Season => PlexMediaType.Season,
            GetMediaMetaDataType.Episode => PlexMediaType.Episode,
            GetMediaMetaDataType.Artist => PlexMediaType.Artist,
            GetMediaMetaDataType.Album => PlexMediaType.Album,
            GetMediaMetaDataType.Track => PlexMediaType.Song,
            GetMediaMetaDataType.PhotoAlbum => PlexMediaType.PhotoAlbum,
            GetMediaMetaDataType.Photo => PlexMediaType.Photos,
            _ => throw new NotImplementedException(
                $"Conversion from {typeof(GetMediaMetaDataType).Name} {source.ToString()} to {nameof(PlexMediaType)} is not implemented."
            ),
        };
}
