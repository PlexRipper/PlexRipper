using Reaparr.Environment;

namespace Reaparr.Domain;

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

    public static string ToDefaultDestinationLocation(this PlexMediaType type)
    {
        return type switch
        {
            PlexMediaType.None => PathProvider.DefaultDownloadsDestinationFolder,
            PlexMediaType.Movie => PathProvider.DefaultMovieDestinationFolder,
            PlexMediaType.TvShow => PathProvider.DefaultTvShowsDestinationFolder,
            PlexMediaType.Season => PathProvider.DefaultTvShowsDestinationFolder,
            PlexMediaType.Episode => PathProvider.DefaultTvShowsDestinationFolder,
            PlexMediaType.Music => PathProvider.DefaultMusicDestinationFolder,
            PlexMediaType.Album => PathProvider.DefaultMusicDestinationFolder,
            PlexMediaType.Song => PathProvider.DefaultMusicDestinationFolder,
            PlexMediaType.Photos => PathProvider.DefaultPhotosDestinationFolder,
            PlexMediaType.OtherVideos => PathProvider.DefaultOtherDestinationFolder,
            PlexMediaType.Games => PathProvider.DefaultGamesDestinationFolder,
            _ => PathProvider.DefaultDownloadsDestinationFolder,
        };
    }
}
