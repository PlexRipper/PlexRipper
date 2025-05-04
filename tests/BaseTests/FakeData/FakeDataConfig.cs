namespace PlexRipper.BaseTests;

public class FakeDataConfig : BaseConfig<FakeDataConfig>
{
    public int PlexServerCount { get; set; } = 1;

    public int PlexServerConnectionPerServerCount { get; set; } = 4;

    public int PlexMovieLibraryCount { get; set; } = 0;
    public int PlexTvShowLibraryCount { get; set; } = 0;

    /// <summary>
    /// The number of PlexAccounts to create which will have access to every PlexServer and PlexLibrary by default.
    /// </summary>
    public int PlexAccountCount { get; set; } = 0;

    public int MovieCount { get; set; } = 0;

    public int TvShowCount { get; set; } = 0;

    public int TvShowSeasonCount { get; set; } = 0;

    public int TvShowEpisodeCount { get; set; } = 0;

    #region DownloadTasks

    public int MovieDownloadTasksCount { get; set; } = 0;

    public int TvShowDownloadTasksCount { get; set; } = 0;

    public int TvShowSeasonDownloadTasksCount { get; set; } = 0;

    public int TvShowEpisodeDownloadTasksCount { get; set; } = 0;

    /// <summary>
    /// NOTE: Setting this number to default to anything other than 0 will cause the unit test to fail/become inconclusive.
    /// </summary>
    public int DownloadWorkerTasks { get; set; } = 0;

    #endregion

    public bool IncludeMultiPartMovies { get; set; }

    public bool IncludeMultiPartEpisodes { get; set; }

    public bool AccountHasAccessToAllLibraries { get; set; }

    public int DownloadFileSizeInMb { get; set; } = 10;

    public bool ShouldHavePlexServer => PlexServerCount > 0 || ShouldHavePlexLibrary;

    public bool ShouldHavePlexLibrary => ShouldHaveMoviePlexLibrary || ShouldHaveTvShowPlexLibrary;

    public bool ShouldHaveMoviePlexLibrary =>
        PlexMovieLibraryCount > 0 || MovieCount > 0 || MovieDownloadTasksCount > 0;

    public bool ShouldHaveTvShowPlexLibrary =>
        PlexTvShowLibraryCount > 0
        || TvShowCount > 0
        || TvShowSeasonCount > 0
        || TvShowEpisodeCount > 0
        || TvShowDownloadTasksCount > 0
        || TvShowSeasonDownloadTasksCount > 0
        || TvShowEpisodeDownloadTasksCount > 0;
}
