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
}