namespace PlexRipper.BaseTests;

public static class MemoryDbContextExtensions
{
    /// <summary>
    /// Sets all navigation properties to null
    /// </summary>
    /// <param name="downloadTasks"></param>
    /// <returns></returns>
    public static List<DownloadTask> SetToNull(this List<DownloadTask> downloadTasks)
    {
        foreach (var downloadTask in downloadTasks)
        {
            downloadTask.PlexServer = null;
            downloadTask.PlexLibrary = null;
            downloadTask.DestinationFolder = null;
            downloadTask.DownloadFolder = null;
            downloadTask.Parent = null;
        }

        return downloadTasks;
    }

    /// <summary>
    /// Sets all navigation properties to null
    /// </summary>
    /// <param name="tvShows"></param>
    /// <returns></returns>
    public static List<PlexTvShow> SetToNull(this List<PlexTvShow> tvShows)
    {
        foreach (var tvShow in tvShows)
        {
            tvShow.PlexServer = null;
            tvShow.PlexLibrary = null;
            tvShow.Seasons = tvShow.Seasons.SetToNull();
        }

        return tvShows;
    }

    /// <summary>
    /// Sets all navigation properties to null
    /// </summary>
    /// <param name="tvShowSeasons"></param>
    /// <returns></returns>
    public static List<PlexTvShowSeason> SetToNull(this List<PlexTvShowSeason> tvShowSeasons)
    {
        foreach (var season in tvShowSeasons)
        {
            season.PlexServer = null;
            season.PlexLibrary = null;
            season.Episodes = season.Episodes.SetToNull();
        }

        return tvShowSeasons;
    }

    /// <summary>
    /// Sets all navigation properties to null
    /// </summary>
    /// <param name="tvShowEpisodes"></param>
    /// <returns></returns>
    public static List<PlexTvShowEpisode> SetToNull(this List<PlexTvShowEpisode> tvShowEpisodes)
    {
        foreach (var tvShow in tvShowEpisodes)
        {
            tvShow.PlexServer = null;
            tvShow.PlexLibrary = null;
        }

        return tvShowEpisodes;
    }
}