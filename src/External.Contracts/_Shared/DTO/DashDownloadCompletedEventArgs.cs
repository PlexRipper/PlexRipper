namespace Reaparr.External.Contracts;

/// <summary>
/// Completion payload for a dash-mpd-cli execution.
/// </summary>
public sealed record DashDownloadCompletedEventArgs(bool Cancelled, int? ExitCode, Result Result)
{
    /// <summary>
    /// Gets a value indicating whether the process completed successfully.
    /// </summary>
    public bool IsSuccess => !Cancelled && Result.IsSuccess;
}
