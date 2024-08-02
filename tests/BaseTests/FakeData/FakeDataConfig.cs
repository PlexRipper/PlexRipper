namespace PlexRipper.BaseTests;

public class FakeDataConfig : BaseConfig<FakeDataConfig>
{
    public int PlexServerCount { get; set; } = 0;

    public int PlexLibraryCount { get; set; } = 0;

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

    #endregion

    public bool IncludeMultiPartMovies { get; set; }

    public bool AccountHasAccessToAllLibraries { get; set; }

    public List<Uri> MockServerUris { get; set; } = new();
    public int DownloadFileSizeInMb { get; set; }

    public bool ShouldHavePlexServer => PlexServerCount > 0 || ShouldHavePlexLibrary;

    public bool ShouldHavePlexLibrary =>
        PlexLibraryCount > 0 || ShouldHaveMoviePlexLibrary || ShouldHaveTvShowPlexLibrary;

    public bool ShouldHaveMoviePlexLibrary => MovieCount > 0 || MovieDownloadTasksCount > 0;

    public bool ShouldHaveTvShowPlexLibrary =>
        TvShowCount > 0
        || TvShowSeasonCount > 0
        || TvShowEpisodeCount > 0
        || TvShowDownloadTasksCount > 0
        || TvShowSeasonDownloadTasksCount > 0
        || TvShowEpisodeDownloadTasksCount > 0;
}
