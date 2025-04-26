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
            "artist" => PlexMediaType.Music,
            "season" => PlexMediaType.Season,
            "episode" => PlexMediaType.Episode,
            "music" => PlexMediaType.Music,
            "album" => PlexMediaType.Album,
            "track" => PlexMediaType.Song,
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

        var result = sourceString switch
        {
            // GetAllLibrariesType is used as all media type enums are the same, just with different names in the SDK
            nameof(GetAllLibrariesType.TvShow) => PlexMediaType.TvShow,
            nameof(GetAllLibrariesType.Movie) => PlexMediaType.Movie,
            nameof(GetAllLibrariesType.Artist) => PlexMediaType.Music,
            nameof(GetAllLibrariesType.Season) => PlexMediaType.Season,
            nameof(GetAllLibrariesType.Episode) => PlexMediaType.Episode,
            nameof(GetAllLibrariesType.Album) => PlexMediaType.Album,
            _ => PlexMediaType.Unknown,
        };

        if (result == PlexMediaType.Unknown)
        {
            _log.Error("Unknown PlexMediaType: {Source}", sourceString);
        }

        return result;
    }

    public static string ToPlexApiString(this PlexMediaType source)
    {
        return source switch
        {
            PlexMediaType.Movie => "movie",
            PlexMediaType.TvShow => "show",
            PlexMediaType.Music => "artist",
            PlexMediaType.Season => "season",
            PlexMediaType.Episode => "episode",
            PlexMediaType.Album => "album",
            PlexMediaType.Song => "track",
            _ => "unknown",
        };
    }
}
