using System.Runtime.Serialization;

namespace PlexRipper.WebAPI;

public enum MessageTypes
{
    [EnumMember(Value = nameof(LibraryProgress))]
    LibraryProgress = 0,

    [EnumMember(Value = nameof(DownloadTaskCreationProgress))]
    DownloadTaskCreationProgress = 1,

    [EnumMember(Value = nameof(DownloadTaskUpdate))]
    DownloadTaskUpdate = 2,

    [EnumMember(Value = nameof(ServerDownloadProgress))]
    ServerDownloadProgress = 3,

    [EnumMember(Value = nameof(InspectServerProgress))]
    InspectServerProgress = 4,

    [EnumMember(Value = nameof(ServerConnectionCheckStatusProgress))]
    ServerConnectionCheckStatusProgress = 5,

    [EnumMember(Value = nameof(FileMergeProgress))]
    FileMergeProgress = 6,

    [EnumMember(Value = nameof(SyncServerMediaProgress))]
    SyncServerMediaProgress = 7,

    [EnumMember(Value = nameof(Notification))]
    Notification = 8,

    [EnumMember(Value = nameof(JobStatusUpdate))]
    JobStatusUpdate = 9,

    [EnumMember(Value = nameof(RefreshNotification))]
    RefreshNotification = 10,
}
