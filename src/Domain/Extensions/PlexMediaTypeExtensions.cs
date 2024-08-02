namespace PlexRipper.Domain;

public static class PlexMediaTypeExtensions
{
    public static int ToDefaultDestinationFolderId(this PlexMediaType type)
    {
        switch (type)
        {
            case PlexMediaType.Movie:
                return 2;
            case PlexMediaType.TvShow:
                return 3;
            case PlexMediaType.Music:
                return 4;
            case PlexMediaType.Photos:
                return 5;
            case PlexMediaType.OtherVideos:
                return 6;
            case PlexMediaType.Games:
                return 7;
            default:
                return 1;
        }
    }
}
