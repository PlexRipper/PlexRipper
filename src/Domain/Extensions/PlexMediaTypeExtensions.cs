namespace PlexRipper.Domain;

public static class PlexMediaTypeExtensions
{
    public static int ToDefaultDestinationFolderId(this PlexMediaType type)
    {
        return type switch
        {
            PlexMediaType.Movie => 2,
            PlexMediaType.TvShow => 3,
            PlexMediaType.Music => 4,
            PlexMediaType.Photos => 5,
            PlexMediaType.OtherVideos => 6,
            PlexMediaType.Games => 7,
            _ => 1,
        };
    }
}
