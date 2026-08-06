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
            Movies = left.Movies + right.Movies,
            TvShows = left.TvShows + right.TvShows,
            Seasons = left.Seasons + right.Seasons,
            Episodes = left.Episodes + right.Episodes,
        };
}
