using System.Runtime.Serialization;

namespace PlexRipper.WebAPI;

/// <summary>
///  Message types for SignalR communication from server to client.
/// </summary>
public enum MessageTypes
{
    /// <summary>
    ///  Library progress message type.
    /// </summary>
    [EnumMember(Value = nameof(LibraryProgress))]
    LibraryProgress = 0,

    /// <summary>
    ///  Download task progress message type.
    /// </summary>
    [EnumMember(Value = nameof(DownloadTaskUpdate))]
    DownloadTaskUpdate = 2,

    /// <summary>
    ///  Server download progress message type.
    /// </summary>
    [EnumMember(Value = nameof(ServerDownloadProgress))]
    ServerDownloadProgress = 3,

    /// <summary>
    ///  Server connection check status message type.
    /// </summary>
    [EnumMember(Value = nameof(ServerConnectionCheckStatusProgress))]
    ServerConnectionCheckStatusProgress = 5,

    /// <summary>
    ///  File merge progress message type.
    /// </summary>
    [EnumMember(Value = nameof(FileMergeProgress))]
    FileMergeProgress = 6,

    /// <summary>
    ///  Sync server media progress message type.
    /// </summary>
    [EnumMember(Value = nameof(SyncServerMediaProgress))]
    SyncServerMediaProgress = 7,

    /// <summary>
    ///  Notification message type.
    /// </summary>
    [EnumMember(Value = nameof(Notification))]
    Notification = 8,

    /// <summary>
    ///  Job status update message type.
    /// </summary>
    [EnumMember(Value = nameof(JobStatusUpdate))]
    JobStatusUpdate = 9,

    /// <summary>
    ///  Refresh notification message type.
    /// </summary>
    [EnumMember(Value = nameof(RefreshNotification))]
    RefreshNotification = 10,
}
