using System.Runtime.Serialization;

namespace Reaparr.Application;

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
    ///  Download the task progress message type.
    /// </summary>
    [EnumMember(Value = nameof(DownloadTaskUpdate))]
    DownloadTaskUpdate = 2,

    /// <summary>
    ///  Server download progress message type.
    /// </summary>
    [EnumMember(Value = nameof(ServerDownloadProgress))]
    ServerDownloadProgress = 3,

    /// <summary>
    ///  Server connection checks status message type.
    /// </summary>
    [EnumMember(Value = nameof(ServerConnectionCheckStatusProgress))]
    ServerConnectionCheckStatusProgress = 5,

    /// <summary>
    ///  File merge progress message type.
    /// </summary>
    [EnumMember(Value = nameof(MoveDownloadFileProgress))]
    MoveDownloadFileProgress = 6,

    /// <summary>
    ///  Notification message type.
    /// </summary>
    [EnumMember(Value = nameof(Notification))]
    Notification = 7,

    /// <summary>
    ///  Job status update message type.
    /// </summary>
    [EnumMember(Value = nameof(JobStatusUpdate))]
    JobStatusUpdate = 8,

    /// <summary>
    ///  Refresh the notification message type.
    /// </summary>
    [EnumMember(Value = nameof(RefreshNotification))]
    RefreshNotification = 9,
}
