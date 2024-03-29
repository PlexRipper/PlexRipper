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
    [EnumMember(Value = "Unknown")]
    Unknown = 0,

    /// <summary>
    /// There was an error during download.
    /// </summary>
    [EnumMember(Value = "Error")]
    Error = 1,

    /// <summary>
    /// Download is added to the queue.
    /// </summary>
    [EnumMember(Value = "Queued")]
    Queued = 2,

    /// <summary>
    /// Download Task is downloading data from the server.
    /// </summary>
    [EnumMember(Value = "Downloading")]
    Downloading = 3,

    /// <summary>
    /// Download Task is downloading data from the server.
    /// </summary>
    [EnumMember(Value = "DownloadFinished")]
    DownloadFinished = 4,

    /// <summary>
    /// Download is paused.
    /// </summary>
    [EnumMember(Value = "Paused")]
    Paused = 5,

    /// <summary>
    /// Download is paused.
    /// </summary>
    [EnumMember(Value = "Stopped")]
    Stopped = 6,

    /// <summary>
    /// Download is deleted.
    /// </summary>
    [EnumMember(Value = "Deleted")]
    Deleted = 7,

    /// <summary>
    /// Download segments are being merged into 1 media file.
    /// </summary>
    [EnumMember(Value = "Merging")]
    Merging = 8,

    /// <summary>
    /// Download file is being moved.
    /// </summary>
    [EnumMember(Value = "Moving")]
    Moving = 9,

    /// <summary>
    /// Download is completed.
    /// </summary>
    [EnumMember(Value = "Completed")]
    Completed = 10,
}