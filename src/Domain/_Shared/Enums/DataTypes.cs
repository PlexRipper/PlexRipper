namespace Reaparr.Domain;

/// <summary>
/// Used to define the type of data being sent.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RefreshDataType
{
    [EnumMember(Value = nameof(PlexAccount))]
    PlexAccount = 0,

    [EnumMember(Value = nameof(PlexServer))]
    PlexServer = 1,

    [EnumMember(Value = nameof(PlexLibrary))]
    PlexLibrary = 2,

    [EnumMember(Value = nameof(PlexLibrarySyncStatus))]
    PlexLibrarySyncStatus = 3,

    [EnumMember(Value = nameof(PlexServerConnection))]
    PlexServerConnection = 4,

    [EnumMember(Value = nameof(DownloadTasks))]
    DownloadTasks = 5,

    [EnumMember(Value = nameof(UpdateAvailable))]
    UpdateAvailable = 6,
}
