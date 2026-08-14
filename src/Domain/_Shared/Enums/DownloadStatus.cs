namespace Reaparr.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DownloadStatus
{
    // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc. and that there is no skip in between
    // Otherwise the TypeScript DTO translator in the front-end starts messing up

    /// <summary>
    /// Fallback value used when a status string cannot be parsed into a known <see cref="DownloadStatus"/> value.
    /// Indicates invalid or unknown persisted/incoming status data.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Unknown))]
    Unknown = 0,

    /// <summary>
    /// Generic download failure bucket when no more specific status applies.
    /// Used as a fallback error state.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Error))]
    Error = 1,

    /// <summary>
    /// Waiting in the download queue and not currently being processed.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Queued))]
    Queued = 2,

    /// <summary>
    /// Actively downloading media data from the source server.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Downloading))]
    Downloading = 3,

    /// <summary>
    /// Download phase has finished and payload data is fully received.
    /// Next step is typically file move/post-processing.
    /// </summary>
    [JsonStringEnumMemberName(nameof(DownloadFinished))]
    DownloadFinished = 4,

    /// <summary>
    /// Downloaded files are currently being moved to their final destination.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Moving))]
    Moving = 5,

    /// <summary>
    /// Move download file to destination phase is paused before completion.
    /// </summary>
    [JsonStringEnumMemberName(nameof(MovePaused))]
    MovePaused = 6,

    /// <summary>
    /// Move process is auto-paused by the system (shutdown/crash recovery) and can be auto-resumed.
    /// </summary>
    [JsonStringEnumMemberName(nameof(AutoMovePaused))]
    AutoMovePaused = 7,

    /// <summary>
    /// Move download file to destination phase has completed successfully.
    /// </summary>
    [JsonStringEnumMemberName(nameof(MoveFinished))]
    MoveFinished = 8,

    /// <summary>
    /// Full download task workflow completed successfully.
    /// Terminal success state.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Completed))]
    Completed = 9,

    /// <summary>
    /// Task is paused and can typically be resumed without resetting progress.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Paused))]
    Paused = 10,

    /// <summary>
    /// Download is auto-paused by the system (shutdown/crash recovery) and can be auto-resumed.
    /// </summary>
    [JsonStringEnumMemberName(nameof(AutoPaused))]
    AutoPaused = 11,

    /// <summary>
    /// Task was explicitly stopped/cancelled and is no longer progressing.
    /// Restart is required to continue.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Stopped))]
    Stopped = 12,

    /// <summary>
    /// Task has been deleted and must not be processed further.
    /// Terminal removed state.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Deleted))]
    Deleted = 13,

    /// <summary>
    /// Source server was unreachable (offline or transient network failure).
    /// Recoverable when connectivity is restored.
    /// </summary>
    [JsonStringEnumMemberName(nameof(ServerUnreachable))]
    ServerUnreachable = 14,

    /// <summary>
    /// Authentication/authorization failed against the source.
    /// User intervention is typically required before retry succeeds.
    /// </summary>
    [JsonStringEnumMemberName(nameof(AuthError))]
    AuthError = 15,

    /// <summary>
    /// Local storage could not be accessed or lacked required capacity/permissions.
    /// </summary>
    [JsonStringEnumMemberName(nameof(StorageError))]
    StorageError = 16,

    /// <summary>
    /// Requested source media is unavailable at origin (missing, inaccessible, or removed).
    /// </summary>
    [JsonStringEnumMemberName(nameof(SourceUnavailable))]
    SourceUnavailable = 17,

    /// <summary>
    /// Download execution failed inside the client pipeline (for example segment/mux/tool/process failure).
    /// Use when failure is client-side and should be distinguished from <see cref="ServerUnreachable"/> and <see cref="SourceUnavailable"/>.
    /// </summary>
    [JsonStringEnumMemberName(nameof(DownloadClientError))]
    DownloadClientError = 18,

    /// <summary>
    /// Downloaded content failed integrity verification after transfer completed.
    /// Use when bytes were received but validation of expected file correctness failed and a clean re-download is required.
    /// </summary>
    [JsonStringEnumMemberName(nameof(IntegrityError))]
    IntegrityError = 19,

    /// <summary>
    /// Move/post-download relocation failed while transferring files to destination.
    /// </summary>
    [JsonStringEnumMemberName(nameof(MoveError))]
    MoveError = 20,

    /// <summary>
    /// Task is in a transient restart transition before re-entering normal processing.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Restarting))]
    Restarting = 21,
}
