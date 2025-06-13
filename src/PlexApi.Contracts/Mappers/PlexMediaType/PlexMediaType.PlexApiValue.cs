using LukeHagar.PlexAPI.SDK.Models.Requests;
using PlexRipper.Domain;

namespace PlexApi.Contracts;

public static partial class PlexMediaTypeMappers
{
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

    public static PlexMediaType ToPlexMediaTypeFromPlexApi(this string source)
    {
        return source.ToLowerInvariant() switch
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
            _ => throw new NotImplementedException(
                $"Conversion from string '{source}' to {nameof(PlexMediaType)} is not implemented."
            ),
        };
    }
}
