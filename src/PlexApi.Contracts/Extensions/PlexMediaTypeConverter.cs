using Logging;
using Logging.Interface;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using PlexRipper.Domain;

namespace PlexApi.Contracts;

public static class StringToPlexMediaTypeConverter
{
    private static ILog _log = LogManager.CreateLogInstance(typeof(StringToPlexMediaTypeConverter));

    public static PlexMediaType ToPlexMediaTypeFromPlexApi(this string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            _log.Error("Parameter {Name} is null or empty, returning {Value}", nameof(source), PlexMediaType.Unknown);
            return PlexMediaType.Unknown;
        }

        var normalized = source.ToLowerInvariant();

        var result = normalized switch
        {
            "movie" => PlexMediaType.Movie,
            "show" => PlexMediaType.TvShow,
            "season" => PlexMediaType.Season,
            "episode" => PlexMediaType.Episode,
            "artist" => PlexMediaType.Artist,
            "album" => PlexMediaType.Album,
            "track" => PlexMediaType.Song,
            "photoalbum" => PlexMediaType.PhotoAlbum,
            "photo" => PlexMediaType.Photos,
            _ => PlexMediaType.Unknown,
        };

        if (result == PlexMediaType.Unknown)
        {
            _log.Error("Unknown PlexMediaType from source string: {Source}", source);
        }

        return result;
    }

    public static PlexMediaType ToPlexMediaTypeFromPlexApi<TEnum>(this TEnum source)
        where TEnum : Enum
    {
        var sourceString = source.ToString();

        return sourceString switch
        {
            // GetAllLibrariesType is used as all media type enums are the same, just with different names in the SDK
            nameof(GetAllLibrariesType.Movie) => PlexMediaType.Movie,
            nameof(GetAllLibrariesType.TvShow) => PlexMediaType.TvShow,
            nameof(GetAllLibrariesType.Season) => PlexMediaType.Season,
            nameof(GetAllLibrariesType.Episode) => PlexMediaType.Episode,
            nameof(GetAllLibrariesType.Artist) => PlexMediaType.Artist,
            nameof(GetAllLibrariesType.Album) => PlexMediaType.Album,
            nameof(GetAllLibrariesType.Track) => PlexMediaType.Song,
            nameof(GetAllLibrariesType.PhotoAlbum) => PlexMediaType.PhotoAlbum,
            nameof(GetAllLibrariesType.Photo) => PlexMediaType.Photos,
            _ => throw new NotImplementedException(
                $"Conversion from {typeof(TEnum).Name} {sourceString} to {nameof(PlexMediaType)} is not implemented."
            ),
        };
    }

    public static GetAllLibrariesType ToGetAllLibrariesType(this PlexMediaType source)
    {
        return source switch
        {
            PlexMediaType.Movie => GetAllLibrariesType.Movie,
            PlexMediaType.TvShow => GetAllLibrariesType.TvShow,
            PlexMediaType.Season => GetAllLibrariesType.Season,
            PlexMediaType.Episode => GetAllLibrariesType.Episode,
            PlexMediaType.Artist => GetAllLibrariesType.Artist,
            PlexMediaType.Album => GetAllLibrariesType.Album,
            PlexMediaType.Song => GetAllLibrariesType.Track,
            PlexMediaType.PhotoAlbum => GetAllLibrariesType.PhotoAlbum,
            PlexMediaType.Photos => GetAllLibrariesType.Photo,
            _ => throw new NotImplementedException(
                $"Conversion from PlexMediaType {source} to GetAllLibrariesType is not implemented."
            ),
        };
    }

    public static string ToPlexApiString(this PlexMediaType source)
    {
        return source switch
        {
            PlexMediaType.Movie => "movie",
            PlexMediaType.TvShow => "show",
            PlexMediaType.Season => "season",
            PlexMediaType.Episode => "episode",
            PlexMediaType.Artist => "artist",
            PlexMediaType.Album => "album",
            PlexMediaType.Song => "track",
            PlexMediaType.PhotoAlbum => "photoalbum",
            PlexMediaType.Photos => "photo",
            _ => throw new NotImplementedException(
                $"Conversion from PlexMediaType {source} to string is not implemented."
            ),
        };
    }
}
