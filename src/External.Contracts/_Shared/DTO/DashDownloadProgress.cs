namespace Reaparr.External.Contracts;

/// <summary>
/// Represents parsed progress data from dash-mpd-cli output.
/// </summary>
public record DashDownloadProgress
{
    public required TimeSpan ETA { get; init; }

    /// <summary>
    /// Gets the percentage of download completed (0-100).
    /// </summary>
    public required int Percent { get; init; }

    /// <summary>
    /// Gets the current download step or operation description.
    /// </summary>
    public string CurrentStep { get; init; } = string.Empty;

    /// <summary>
    /// Gets the download speed in bytes per second.
    /// </summary>
    public required long DownloadSpeedInBytes { get; init; }

    public required long DownloadedBytes { get; init; }

    public required long TotalBytes { get; init; }

    /// <summary>
    /// Gets the raw output line that was parsed.
    /// </summary>
    public required string RawOutput { get; init; } = string.Empty;
}
