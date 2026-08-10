namespace Reaparr.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum JobTypes
{
    // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc. and that there is no skip in between
    // Otherwise the TypeScript DTO translator in the front-end starts messing up
    [JsonStringEnumMemberName(nameof(Unknown))]
    Unknown = 0,

    [JsonStringEnumMemberName(nameof(CheckAllConnectionsStatusByPlexServerJob))]
    CheckAllConnectionsStatusByPlexServerJob = 1,

    [JsonStringEnumMemberName(nameof(DownloadJob))]
    DownloadJob = 2,

    [JsonStringEnumMemberName(nameof(MoveDownloadFileJob))]
    MoveDownloadFileJob = 3,

    [JsonStringEnumMemberName(nameof(InspectPlexServerJob))]
    InspectPlexServerJob = 4,

    [JsonStringEnumMemberName(nameof(LibrarySyncJob))]
    LibrarySyncJob = 5,

    [JsonStringEnumMemberName(nameof(MetadataSyncJob))]
    MetadataSyncJob = 6,

    [JsonStringEnumMemberName(nameof(CheckForUpdateJob))]
    CheckForUpdateJob = 7,

    [JsonStringEnumMemberName(nameof(CheckPlexLibrariesForUpdatesJob))]
    CheckPlexLibrariesForUpdatesJob = 8,

    [JsonStringEnumMemberName(nameof(LibraryComparisonJob))]
    LibraryComparisonJob = 9,

    [JsonStringEnumMemberName(nameof(RefreshPlexAccountAccessJob))]
    RefreshPlexAccountAccessJob = 10,

    // Ensure to add new job types to ToJobStatusUpdate in JobExecutionContextExtensions
}
