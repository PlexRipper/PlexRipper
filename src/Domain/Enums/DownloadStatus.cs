using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace PlexRipper.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DownloadStatus
{
    // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc and that there is no skip in between
    // Otherwise the Typescript DTO translator in the front-end starts messing up

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
    /// Download Task is downloading data from the server.
    /// </summary>
    [EnumMember(Value = nameof(Downloading))]
    Downloading = 3,

    /// <summary>
    /// Download Task is downloading data from the server.
    /// </summary>
    [EnumMember(Value = nameof(DownloadFinished))]
    DownloadFinished = 4,

    /// <summary>
    /// Download is paused.
    /// </summary>
    [EnumMember(Value = nameof(Paused))]
    Paused = 5,

    /// <summary>
    /// Download is paused.
    /// </summary>
    [EnumMember(Value = nameof(Stopped))]
    Stopped = 6,

    /// <summary>
    /// Download is deleted.
    /// </summary>
    [EnumMember(Value = nameof(Deleted))]
    Deleted = 7,

    /// <summary>
    /// Download segments are being merged into 1 media file.
    /// </summary>
    [EnumMember(Value = nameof(Merging))]
    Merging = 8,

    /// <summary>
    /// Download file is being moved.
    /// </summary>
    [EnumMember(Value = nameof(Moving))]
    Moving = 9,

    /// <summary>
    /// Download file has been merged.
    /// </summary>
    [EnumMember(Value = nameof(MergePaused))]
    MergePaused = 10,

    /// <summary>
    /// Download file has been moved.
    /// </summary>
    [EnumMember(Value = nameof(MovePaused))]
    MovePaused = 11,

    /// <summary>
    /// Download file has been merged.
    /// </summary>
    [EnumMember(Value = nameof(MergeFinished))]
    MergeFinished = 12,

    /// <summary>
    /// Download file has been moved.
    /// </summary>
    [EnumMember(Value = nameof(MoveFinished))]
    MoveFinished = 13,

    /// <summary>
    /// Download is completed.
    /// </summary>
    [EnumMember(Value = nameof(Completed))]
    Completed = 14,

    /// <summary>
    /// The server is offline.
    /// </summary>
    [EnumMember(Value = nameof(ServerUnreachable))]
    ServerUnreachable = 15,

    /// <summary>
    /// The server is offline.
    /// </summary>
    [EnumMember(Value = nameof(MoveError))]
    MoveError = 16,

    /// <summary>
    /// The server is offline.
    /// </summary>
    [EnumMember(Value = nameof(MergeError))]
    MergeError = 17,
}
