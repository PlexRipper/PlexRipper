namespace PlexRipper.Domain;

public static partial class EnumMapperExtensions
{
    private static readonly Dictionary<string, DownloadStatus> _stringToStatus =
        new(StringComparer.Ordinal)
        {
            ["Unknown"] = DownloadStatus.Unknown,
            ["Error"] = DownloadStatus.Error,
            ["Queued"] = DownloadStatus.Queued,
            ["Downloading"] = DownloadStatus.Downloading,
            ["DownloadFinished"] = DownloadStatus.DownloadFinished,
            ["Paused"] = DownloadStatus.Paused,
            ["Stopped"] = DownloadStatus.Stopped,
            ["Deleted"] = DownloadStatus.Deleted,
            ["Merging"] = DownloadStatus.Merging,
            ["Moving"] = DownloadStatus.Moving,
            ["MergePaused"] = DownloadStatus.MergePaused,
            ["MovePaused"] = DownloadStatus.MovePaused,
            ["MergeFinished"] = DownloadStatus.MergeFinished,
            ["MoveFinished"] = DownloadStatus.MoveFinished,
            ["Completed"] = DownloadStatus.Completed,
            ["ServerUnreachable"] = DownloadStatus.ServerUnreachable,
            ["MoveError"] = DownloadStatus.MoveError,
            ["MergeError"] = DownloadStatus.MergeError,
        };

    /// <summary>
    /// Converts string to <see cref="DownloadStatus"/> by a fast method.
    /// </summary>
    /// <param name="value">The string representation of <see cref="DownloadStatus"/>.</param>
    /// <returns>The converted enum of type <see cref="DownloadStatus"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws exception if value is not found.</exception>
    public static DownloadStatus ToDownloadStatus(this string value)
    {
        if (_stringToStatus.TryGetValue(value, out var status))
            return status;

        _log.Here().Error("Failed to convert string {Value} to {DownloadStatus}", value, nameof(DownloadStatus));
        throw new ArgumentOutOfRangeException(nameof(value), value, null);
    }

    /// <summary>
    /// Converts <see cref="DownloadStatus"/> to string by a fast method.
    /// </summary>
    /// <param name="value">The enum of type <see cref="DownloadStatus"/>.</param>
    /// <returns>The string value of the <see cref="DownloadStatus"/> property.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws exception if value is not found.</exception>
    public static string ToDownloadStatusString(this DownloadStatus value)
    {
        return value switch
        {
            DownloadStatus.Unknown => "Unknown",
            DownloadStatus.Error => "Error",
            DownloadStatus.Queued => "Queued",
            DownloadStatus.Downloading => "Downloading",
            DownloadStatus.DownloadFinished => "DownloadFinished",
            DownloadStatus.Paused => "Paused",
            DownloadStatus.Stopped => "Stopped",
            DownloadStatus.Deleted => "Deleted",
            DownloadStatus.Merging => "Merging",
            DownloadStatus.Moving => "Moving",
            DownloadStatus.MergePaused => "MergePaused",
            DownloadStatus.MovePaused => "MovePaused",
            DownloadStatus.MergeFinished => "MergeFinished",
            DownloadStatus.MoveFinished => "MoveFinished",
            DownloadStatus.Completed => "Completed",
            DownloadStatus.ServerUnreachable => "ServerUnreachable",
            DownloadStatus.MoveError => "MoveError",
            DownloadStatus.MergeError => "MergeError",
            _ => DefaultException(),
        };

        string DefaultException()
        {
            _log.Here()
                .Error(
                    "Failed to convert {Value} to string of type {NameOfDownloadStatus}",
                    value,
                    nameof(DownloadStatus)
                );
            throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }
}
