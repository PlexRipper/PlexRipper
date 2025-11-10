using LukeHagar.PlexAPI.SDK.Models.Requests;

namespace Reaparr.PlexApi.Contracts;

public static partial class PlexMediaTypeMappers
{
    public static GetLibrarySectionsAllQueryParamType ToGetLibrarySectionsAllQueryParamType(
        this PlexMediaType source
    ) =>
        source switch
        {
            PlexMediaType.Movie => GetLibrarySectionsAllQueryParamType.Movie,
            PlexMediaType.TvShow => GetLibrarySectionsAllQueryParamType.TvShow,
            PlexMediaType.Season => GetLibrarySectionsAllQueryParamType.Season,
            PlexMediaType.Episode => GetLibrarySectionsAllQueryParamType.Episode,
            PlexMediaType.Artist => GetLibrarySectionsAllQueryParamType.Artist,
            PlexMediaType.Album => GetLibrarySectionsAllQueryParamType.Album,
            PlexMediaType.Song => GetLibrarySectionsAllQueryParamType.Track,
            PlexMediaType.PhotoAlbum => GetLibrarySectionsAllQueryParamType.PhotoAlbum,
            PlexMediaType.Photos => GetLibrarySectionsAllQueryParamType.Photo,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, "Unsupported PlexMediaType"),
        };
}
