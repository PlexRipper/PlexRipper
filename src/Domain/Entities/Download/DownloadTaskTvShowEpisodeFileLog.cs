namespace Reaparr.Domain;

public class DownloadTaskTvShowEpisodeFileLog : DownloadTaskLogBase
{
    #region Relationships

    /// <summary>
    /// Gets the <see cref="DownloadTaskTvShowEpisodeFile"/> this log belongs to.
    /// </summary>
    [Column(Order = 5)]
    public required Guid DownloadTaskFileId { get; init; }

    public DownloadTaskTvShowEpisodeFile? DownloadTaskFile { get; init; }

    /// <summary>
    /// Gets the <see cref="DownloadTaskTvShowEpisode"/> this log belongs to.
    /// </summary>
    [Column(Order = 6)]
    public required Guid DownloadTaskTvShowEpisodeId { get; init; }

    public DownloadTaskTvShowEpisode? DownloadTaskTvShowEpisode { get; init; }

    /// <summary>
    /// Gets the <see cref="DownloadTaskTvShowSeason"/> this log belongs to.
    /// </summary>
    [Column(Order = 7)]
    public required Guid DownloadTaskTvShowSeasonId { get; init; }

    public DownloadTaskTvShowSeason? DownloadTaskTvShowSeason { get; init; }

    /// <summary>
    /// Gets the <see cref="DownloadTaskTvShow"/> this log belongs to.
    /// </summary>
    [Column(Order = 8)]
    public required Guid DownloadTaskTvShowId { get; init; }

    public DownloadTaskTvShow? DownloadTaskTvShow { get; init; }

    #endregion
}
