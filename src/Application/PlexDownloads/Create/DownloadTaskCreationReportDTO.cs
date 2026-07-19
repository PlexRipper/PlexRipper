namespace Reaparr.Application;

/// <summary>
/// Reports counts of download tasks created, broken down by media type.
/// </summary>
public sealed record DownloadTaskCreationReportDTO
{
    public required int Movies { get; init; }
    public required int TvShows { get; init; }
    public required int Seasons { get; init; }
    public required int Episodes { get; init; }

    public int Total => Movies + TvShows + Seasons + Episodes;
}
