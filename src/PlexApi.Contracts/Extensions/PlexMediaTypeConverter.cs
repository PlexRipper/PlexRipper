using PlexRipper.Domain;

namespace PlexApi.Contracts;

public static class StringToPlexMediaTypeConverter
{
    public static PlexMediaType ToPlexMediaTypeFromPlexApi(this string source)
    {
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
}
