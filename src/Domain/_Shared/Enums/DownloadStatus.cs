namespace Reaparr.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DownloadStatus
{
    // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc. and that there is no skip in between
    // Otherwise the TypeScript DTO translator in the front-end starts messing up

    /// <summary>
    /// String value was unable to be parsed to this enum.
    /// </summary>
    [EnumMember(Value = nameof(Unknown))]
    Unknown = 0,

    /// <summary>
    /// There was an error during download.
    /// </summary>
    [EnumMember(Value = nameof(Error))]
    Error = 1,

    /// <summary>
    /// Download is added to the queue.
    /// </summary>
    [EnumMember(Value = nameof(Queued))]
    Queued = 2,

    /// <summary>
    /// Download Task has finished downloading data from the server.
    /// </summary>
    [EnumMember(Value = nameof(Downloading))]
    Downloading = 3,

    /// <summary>
    /// Download Task is downloading data from the server.
    /// </summary>
    [EnumMember(Value = nameof(DownloadFinished))]
    DownloadFinished = 4,

    /// <summary>
    /// Download file is being moved.
    /// </summary>
    [EnumMember(Value = nameof(Moving))]
    Moving = 5,

    /// <summary>
    /// Download file is paused during move.
    /// </summary>
    [EnumMember(Value = nameof(MovePaused))]
    MovePaused = 6,

    /// <summary>
    /// Download file has been moved.
    /// </summary>
    [EnumMember(Value = nameof(MoveFinished))]
    MoveFinished = 7,

    /// <summary>
    /// Download is completed.
    /// </summary>
    [EnumMember(Value = nameof(Completed))]
    Completed = 8,

    /// <summary>
    /// Download is paused.
    /// </summary>
    [EnumMember(Value = nameof(Paused))]
    Paused = 9,

    /// <summary>
    /// Download is paused.
    /// </summary>
    [EnumMember(Value = nameof(Stopped))]
    Stopped = 10,

    /// <summary>
    /// Download is deleted.
    /// </summary>
    [EnumMember(Value = nameof(Deleted))]
    Deleted = 11,

    /// <summary>
    /// The server is offline.
    /// </summary>
    [EnumMember(Value = nameof(ServerUnreachable))]
    ServerUnreachable = 12,

    /// <summary>
    /// Authentication failed and user action is required.
    /// </summary>
    [EnumMember(Value = nameof(AuthError))]
    AuthError = 13,

    /// <summary>
    /// Storage could not be accessed or is full.
    /// </summary>
    [EnumMember(Value = nameof(StorageError))]
    StorageError = 14,

    /// <summary>
    /// The media source is unavailable.
    /// </summary>
    [EnumMember(Value = nameof(SourceUnavailable))]
    SourceUnavailable = 15,

    /// <summary>
    /// The download client failed.
    /// </summary>
    [EnumMember(Value = nameof(DownloadClientError))]
    DownloadClientError = 16,

    /// <summary>
    /// Download integrity verification failed.
    /// </summary>
    [EnumMember(Value = nameof(IntegrityError))]
    IntegrityError = 17,

    /// <summary>
    /// The move operation failed.
    /// </summary>
    [EnumMember(Value = nameof(MoveError))]
    MoveError = 18,

    /// <summary>
    /// The DownloadTask is in the process of restarting
    /// </summary>
    [EnumMember(Value = nameof(Restarting))]
    Restarting = 19,
}
