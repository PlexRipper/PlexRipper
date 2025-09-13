namespace Reaparr.PlexApi;

public static class PlexMediaTypeToApiTypeExtensions
{
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
            case PlexMediaType.Artist:
                return (T)Enum.ToObject(typeof(T), "artist");
            case PlexMediaType.Album:
                return (T)Enum.ToObject(typeof(T), "album");
            case PlexMediaType.Song:
                return (T)Enum.ToObject(typeof(T), "track");
            case PlexMediaType.PhotoAlbum:
                return (T)Enum.ToObject(typeof(T), "photoalbum");
            case PlexMediaType.Photos:
                return (T)Enum.ToObject(typeof(T), "photo");
            default:
                throw new ArgumentOutOfRangeException(nameof(plexMediaType), plexMediaType, null);
        }

        return (T)Enum.ToObject(typeof(T), enumInt);
    }
}
