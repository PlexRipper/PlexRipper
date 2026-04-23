namespace Reaparr.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DownloadStatus
{
    // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc. and that there is no skip in between
    // Otherwise the TypeScript DTO translator in the front-end starts messing up

    /// <summary>
    /// String value was unable to be parsed to this enum.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Unknown))]
    Unknown = 0,

    /// <summary>
    /// There was an error during download.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Error))]
    Error = 1,

    /// <summary>
    /// Download is added to the queue.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Queued))]
    Queued = 2,

    /// <summary>
    /// Download Task has finished downloading data from the server.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Downloading))]
    Downloading = 3,

    /// <summary>
    /// Download Task is downloading data from the server.
    /// </summary>
    [JsonStringEnumMemberName(nameof(DownloadFinished))]
    DownloadFinished = 4,

    /// <summary>
    /// Download file is being moved.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Moving))]
    Moving = 5,

    /// <summary>
    /// Download file is paused during move.
    /// </summary>
    [JsonStringEnumMemberName(nameof(MovePaused))]
    MovePaused = 6,

    /// <summary>
    /// Download file has been moved.
    /// </summary>
    [JsonStringEnumMemberName(nameof(MoveFinished))]
    MoveFinished = 7,

    /// <summary>
    /// Download is completed.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Completed))]
    Completed = 8,

    /// <summary>
    /// Download is paused.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Paused))]
    Paused = 9,

    /// <summary>
    /// Download is paused.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Stopped))]
    Stopped = 10,

    /// <summary>
    /// Download is deleted.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Deleted))]
    Deleted = 11,

    /// <summary>
    /// The server is offline.
    /// </summary>
    [JsonStringEnumMemberName(nameof(ServerUnreachable))]
    ServerUnreachable = 12,

    /// <summary>
    /// Authentication failed and user action is required.
    /// </summary>
    [JsonStringEnumMemberName(nameof(AuthError))]
    AuthError = 13,

    /// <summary>
    /// Storage could not be accessed or is full.
    /// </summary>
    [JsonStringEnumMemberName(nameof(StorageError))]
    StorageError = 14,

    /// <summary>
    /// The media source is unavailable.
    /// </summary>
    [JsonStringEnumMemberName(nameof(SourceUnavailable))]
    SourceUnavailable = 15,

    /// <summary>
    /// The download client failed.
    /// </summary>
    [JsonStringEnumMemberName(nameof(DownloadClientError))]
    DownloadClientError = 16,

    /// <summary>
    /// Download integrity verification failed.
    /// </summary>
    [JsonStringEnumMemberName(nameof(IntegrityError))]
    IntegrityError = 17,

    /// <summary>
    /// The move operation failed.
    /// </summary>
    [JsonStringEnumMemberName(nameof(MoveError))]
    MoveError = 18,

    /// <summary>
    /// The DownloadTask is in the process of restarting
    /// </summary>
    [JsonStringEnumMemberName(nameof(Restarting))]
    Restarting = 19,
}
