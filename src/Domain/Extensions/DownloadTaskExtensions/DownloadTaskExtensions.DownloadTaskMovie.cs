namespace PlexRipper.Domain;

public static partial class DownloadTaskExtensions
{
    /// <summary>
    /// This will set the relationship ids for the download tasks and it's children.
    /// </summary>
    public static void SetRelationshipIds(
        this List<DownloadTaskMovie>? downloadTasks,
        int plexServerId,
        int plexLibraryId
    )
    {
        if (downloadTasks is null)
            return;

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
    }

    /// <summary>
    /// This will set the relationship ids for the download tasks and it's children.
    /// </summary>
    public static void SetRelationshipIds(
        this List<DownloadTaskTvShow> downloadTasks,
        int plexServerId,
        int plexLibraryId
    )
    {
        if (downloadTasks is null)
            return;

        foreach (var downloadTaskTvShow in downloadTasks)
        {
            downloadTaskTvShow.PlexLibraryId = plexLibraryId;
            downloadTaskTvShow.PlexServerId = plexServerId;
            downloadTaskTvShow.Children.SetRelationshipIds(plexServerId, plexLibraryId);
        }
    }

    /// <summary>
    /// This will set the relationship ids for the download tasks and it's children.
    /// </summary>
    public static void SetRelationshipIds(
        this List<DownloadTaskTvShowSeason> downloadTasks,
        int plexServerId,
        int plexLibraryId
    )
    {
        if (downloadTasks is null)
            return;

        foreach (var downloadTaskTvShowSeason in downloadTasks)
        {
            downloadTaskTvShowSeason.PlexLibraryId = plexLibraryId;
            downloadTaskTvShowSeason.PlexServerId = plexServerId;
            downloadTaskTvShowSeason.Children.SetRelationshipIds(plexServerId, plexLibraryId);
        }
    }

    /// <summary>
    /// This will set the relationship ids for the download tasks and it's children.
    /// </summary>
    public static void SetRelationshipIds(
        this List<DownloadTaskTvShowEpisode> downloadTasks,
        int plexServerId,
        int plexLibraryId
    )
    {
        if (downloadTasks is null)
            return;

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
    }
}
