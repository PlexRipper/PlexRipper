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
}
