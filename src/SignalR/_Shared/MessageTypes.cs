using System.Runtime.Serialization;

namespace Reaparr.SignalR;

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
    ///  Download patch message type.
    /// </summary>
    [EnumMember(Value = nameof(DownloadPatch))]
    DownloadPatch = 4,

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

    /// <summary>
    ///  App update download progress message type.
    /// </summary>
    [EnumMember(Value = nameof(AppUpdateDownloadProgress))]
    AppUpdateDownloadProgress = 10,

    /// <summary>
    ///  Live log event message type.
    /// </summary>
    [EnumMember(Value = nameof(LogEvent))]
    LogEvent = 11,
}
