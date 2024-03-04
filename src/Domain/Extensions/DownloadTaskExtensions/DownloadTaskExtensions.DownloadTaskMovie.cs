namespace PlexRipper.Domain;

public static partial class DownloadTaskExtensions
{
    /// <summary>
    /// This will set the relationship ids for the download tasks and it's children.
    /// </summary>
    public static List<DownloadTaskMovie> SetRelationshipIds(this List<DownloadTaskMovie> downloadTasks, int plexServerId, int plexLibraryId)
    {
        if (downloadTasks is null)
            return null;

        foreach (var downloadTaskMovie in downloadTasks)
        {
            downloadTaskMovie.PlexLibraryId = plexLibraryId;
            downloadTaskMovie.PlexServerId = plexServerId;
            foreach (var downloadTaskMovieFile in downloadTaskMovie.Children)
            {
                downloadTaskMovieFile.PlexLibraryId = plexLibraryId;
                downloadTaskMovieFile.PlexServerId = plexServerId;
            }
        }

        return downloadTasks;
    }

    /// <summary>
    /// This will set the relationship ids for the download tasks and it's children.
    /// </summary>
    public static List<DownloadTaskTvShow> SetRelationshipIds(this List<DownloadTaskTvShow> downloadTasks, int plexServerId, int plexLibraryId)
    {
        if (downloadTasks is null)
            return null;

        foreach (var downloadTaskTvShow in downloadTasks)
        {
            downloadTaskTvShow.PlexLibraryId = plexLibraryId;
            downloadTaskTvShow.PlexServerId = plexServerId;
            downloadTaskTvShow.Children.SetRelationshipIds(plexServerId, plexLibraryId);
        }

        return downloadTasks;
    }

    /// <summary>
    /// This will set the relationship ids for the download tasks and it's children.
    /// </summary>
    public static List<DownloadTaskTvShowSeason> SetRelationshipIds(this List<DownloadTaskTvShowSeason> downloadTasks, int plexServerId, int plexLibraryId)
    {
        if (downloadTasks is null)
            return null;

        foreach (var downloadTaskTvShowSeason in downloadTasks)
        {
            downloadTaskTvShowSeason.PlexLibraryId = plexLibraryId;
            downloadTaskTvShowSeason.PlexServerId = plexServerId;
            downloadTaskTvShowSeason.Children.SetRelationshipIds(plexServerId, plexLibraryId);
        }

        return downloadTasks;
    }

    /// <summary>
    /// This will set the relationship ids for the download tasks and it's children.
    /// </summary>
    public static List<DownloadTaskTvShowEpisode> SetRelationshipIds(this List<DownloadTaskTvShowEpisode> downloadTasks, int plexServerId, int plexLibraryId)
    {
        if (downloadTasks is null)
            return null;

        foreach (var downloadTaskTvShowEpisode in downloadTasks)
        {
            downloadTaskTvShowEpisode.PlexLibraryId = plexLibraryId;
            downloadTaskTvShowEpisode.PlexServerId = plexServerId;
            foreach (var downloadTaskTvShowEpisodeFile in downloadTaskTvShowEpisode.Children)
            {
                downloadTaskTvShowEpisodeFile.PlexLibraryId = plexLibraryId;
                downloadTaskTvShowEpisodeFile.PlexServerId = plexServerId;
            }
        }

        return downloadTasks;
    }
}