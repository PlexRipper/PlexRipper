namespace PlexRipper.PlexApi;

public static class PlexMediaTypeToApiTypeExtensions
{
    public static T ToApiTypeEnum<T>(this PlexMediaType plexMediaType)
        where T : struct, Enum
    {
        var enumInt = 0;
        switch (plexMediaType)
        {
            case PlexMediaType.None:
                break;
            case PlexMediaType.Movie:
                return (T)Enum.ToObject(typeof(T), 1);
            case PlexMediaType.TvShow:
                return (T)Enum.ToObject(typeof(T), 2);
            case PlexMediaType.Season:
                return (T)Enum.ToObject(typeof(T), 3);
            case PlexMediaType.Episode:
                return (T)Enum.ToObject(typeof(T), 4);
            case PlexMediaType.Music:
                return (T)Enum.ToObject(typeof(T), 8);
            case PlexMediaType.Album:
                return (T)Enum.ToObject(typeof(T), 9);
            case PlexMediaType.Song:
                return (T)Enum.ToObject(typeof(T), 10);
            default:
                throw new ArgumentOutOfRangeException(nameof(plexMediaType), plexMediaType, null);
        }

        return (T)Enum.ToObject(typeof(T), enumInt);
    }

    public static T ToApiTypeEnumFromString<T>(this PlexMediaType plexMediaType)
        where T : struct, Enum
    {
        var enumInt = 0;
        switch (plexMediaType)
        {
            case PlexMediaType.None:
                break;
            case PlexMediaType.Movie:
                return (T)Enum.ToObject(typeof(T), "movie");
            case PlexMediaType.TvShow:
                return (T)Enum.ToObject(typeof(T), "show");
            case PlexMediaType.Season:
                return (T)Enum.ToObject(typeof(T), "season");
            case PlexMediaType.Episode:
                return (T)Enum.ToObject(typeof(T), "episode");
            case PlexMediaType.Music:
                return (T)Enum.ToObject(typeof(T), "music");
            case PlexMediaType.Album:
                return (T)Enum.ToObject(typeof(T), "album");
            case PlexMediaType.Song:
                return (T)Enum.ToObject(typeof(T), "track");
            default:
                throw new ArgumentOutOfRangeException(nameof(plexMediaType), plexMediaType, null);
        }

        return (T)Enum.ToObject(typeof(T), enumInt);
    }

    /// <summary>
    /// Converts a string representation of a Plex media type to the corresponding PlexMediaType enum value.
    /// </summary>
    /// <remarks>NOTE: This method assumes the string is given as an int in string format.</remarks>
    /// <example> 1 => PlexMediaType.Movie</example>
    /// <example> 2 => PlexMediaType.TvShow</example>
    /// <example> 3 => PlexMediaType.Season</example>
    /// <example> 4 => PlexMediaType.Episode</example>
    /// <exception cref="ArgumentOutOfRangeException"> Thrown when the provided string does not match any known Plex media type.</exception>
    public static PlexMediaType ToPlexMediaTypeFromTypeInt(this string apiType)
    {
        if (!int.TryParse(apiType, out var i))
            return PlexMediaType.Unknown;

        return i switch
        {
            1 => PlexMediaType.Movie,
            2 => PlexMediaType.TvShow,
            3 => PlexMediaType.Season,
            4 => PlexMediaType.Episode,
            5 => PlexMediaType.Artist,
            6 => PlexMediaType.Album,
            7 => PlexMediaType.Song,
            8 => PlexMediaType.PhotoAlbum,
            9 => PlexMediaType.Photos,
            _ => throw new ArgumentOutOfRangeException(nameof(apiType), $"Unknown media type value: {apiType}"),
        };
    }
}
