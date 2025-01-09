using PlexRipper.Domain;

namespace PlexApi.Contracts;

public static class StringToPlexMediaTypeConverter
{
    public static PlexMediaType ToPlexMediaTypeFromPlexApi(this string source)
    {
        if (string.IsNullOrEmpty(source))
            return PlexMediaType.Unknown;

        return source.ToLower() switch
        {
            "movie" => PlexMediaType.Movie,
            "show" => PlexMediaType.TvShow,
            "artist" => PlexMediaType.Music,
            "season" => PlexMediaType.Season,
            "episode" => PlexMediaType.Episode,
            "music" => PlexMediaType.Music,
            "album" => PlexMediaType.Album,
            _ => PlexMediaType.Unknown,
        };
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
            _ => "unknown",
        };
    }
}
