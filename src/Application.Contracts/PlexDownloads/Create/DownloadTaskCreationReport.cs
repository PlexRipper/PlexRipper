namespace Reaparr.Application.Contracts;

/// <summary>
/// Reports counts of download tasks created, broken down by media type.
/// </summary>
public sealed record DownloadTaskCreationReport
{
    public int Movies { get; init; }
    public int TvShows { get; init; }
    public int Seasons { get; init; }
    public int Episodes { get; init; }

    public int Total => Movies + TvShows + Seasons + Episodes;

    public static DownloadTaskCreationReport operator +(
        DownloadTaskCreationReport left,
        DownloadTaskCreationReport right
    ) =>
        new()
        {
            Movies = (left?.Movies ?? 0) + (right?.Movies ?? 0),
            TvShows = (left?.TvShows ?? 0) + (right?.TvShows ?? 0),
            Seasons = (left?.Seasons ?? 0) + (right?.Seasons ?? 0),
            Episodes = (left?.Episodes ?? 0) + (right?.Episodes ?? 0),
        };
}
