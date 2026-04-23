namespace Reaparr.Domain;

/// <summary>
/// Used to define the type of data being sent.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RefreshDataType
{
    [JsonStringEnumMemberName(nameof(PlexAccount))]
    PlexAccount = 0,

    [JsonStringEnumMemberName(nameof(PlexServer))]
    PlexServer = 1,

    [JsonStringEnumMemberName(nameof(PlexLibrary))]
    PlexLibrary = 2,

    [JsonStringEnumMemberName(nameof(PlexLibrarySyncStatus))]
    PlexLibrarySyncStatus = 3,

    [JsonStringEnumMemberName(nameof(PlexServerConnection))]
    PlexServerConnection = 4,

    [JsonStringEnumMemberName(nameof(DownloadTasks))]
    DownloadTasks = 5,

    [JsonStringEnumMemberName(nameof(UpdateAvailable))]
    UpdateAvailable = 6,
}
