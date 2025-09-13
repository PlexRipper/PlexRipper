using LukeHagar.PlexAPI.SDK.Models.Requests;
using Reaparr.Domain;

namespace Reaparr.PlexApi.Contracts;

public static partial class PlexMediaTypeMappers
{
    public static GetLibrarySectionsAllLibraryType ToGetLibrarySectionsAllLibraryType(this PlexMediaType source) =>
        source.ToApiTypeEnum<GetLibrarySectionsAllLibraryType>();

    public static PlexMediaType ToPlexMediaType(this GetLibrarySectionsAllLibraryType source) =>
        source switch
        {
            GetLibrarySectionsAllLibraryType.Movie => PlexMediaType.Movie,
            GetLibrarySectionsAllLibraryType.TvShow => PlexMediaType.TvShow,
            GetLibrarySectionsAllLibraryType.Season => PlexMediaType.Season,
            GetLibrarySectionsAllLibraryType.Episode => PlexMediaType.Episode,
            GetLibrarySectionsAllLibraryType.Artist => PlexMediaType.Artist,
            GetLibrarySectionsAllLibraryType.Album => PlexMediaType.Album,
            GetLibrarySectionsAllLibraryType.Track => PlexMediaType.Song,
            GetLibrarySectionsAllLibraryType.PhotoAlbum => PlexMediaType.PhotoAlbum,
            GetLibrarySectionsAllLibraryType.Photo => PlexMediaType.Photos,
            _ => throw new NotImplementedException(
                $"Conversion from {typeof(GetLibrarySectionsAllLibraryType).Name} {source.ToString()} to {nameof(PlexMediaType)} is not implemented."
            ),
        };
}
