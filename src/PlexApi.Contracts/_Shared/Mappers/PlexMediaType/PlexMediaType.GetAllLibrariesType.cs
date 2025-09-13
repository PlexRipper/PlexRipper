using LukeHagar.PlexAPI.SDK.Models.Requests;
using Reaparr.Domain;

namespace Reaparr.PlexApi.Contracts;

public static partial class PlexMediaTypeMappers
{
    public static GetAllLibrariesType ToGetAllLibrariesType(this PlexMediaType source) =>
        source.ToApiTypeEnum<GetAllLibrariesType>();

    public static PlexMediaType ToPlexMediaType(this GetAllLibrariesType source) =>
        source switch
        {
            GetAllLibrariesType.Movie => PlexMediaType.Movie,
            GetAllLibrariesType.TvShow => PlexMediaType.TvShow,
            GetAllLibrariesType.Season => PlexMediaType.Season,
            GetAllLibrariesType.Episode => PlexMediaType.Episode,
            GetAllLibrariesType.Artist => PlexMediaType.Artist,
            GetAllLibrariesType.Album => PlexMediaType.Album,
            GetAllLibrariesType.Track => PlexMediaType.Song,
            GetAllLibrariesType.PhotoAlbum => PlexMediaType.PhotoAlbum,
            GetAllLibrariesType.Photo => PlexMediaType.Photos,
            _ => throw new NotImplementedException(
                $"Conversion from {typeof(GetAllLibrariesType).Name} {source.ToString()} to {nameof(PlexMediaType)} is not implemented."
            ),
        };
}
