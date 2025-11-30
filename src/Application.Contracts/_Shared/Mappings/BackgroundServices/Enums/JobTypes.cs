using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Reaparr.Application.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum JobTypes
{
    // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc. and that there is no skip in between
    // Otherwise the TypeScript DTO translator in the front-end starts messing up
    [EnumMember(Value = "Unknown")]
    Unknown = 0,

    [EnumMember(Value = nameof(CheckAllConnectionsStatusByPlexServerJob))]
    CheckAllConnectionsStatusByPlexServerJob = 1,

    [EnumMember(Value = nameof(DownloadJob))]
    DownloadJob = 2,

    [EnumMember(Value = nameof(MoveDownloadFileJob))]
    MoveDownloadFileJob = 3,

    [EnumMember(Value = nameof(SyncServerMediaJob))]
    SyncServerMediaJob = 4,

    [EnumMember(Value = nameof(InspectPlexServerJob))]
    InspectPlexServerJob = 5,

    [EnumMember(Value = nameof(LibrarySyncJob))]
    LibrarySyncJob = 6,

    // Ensure to add new job types to ToJobStatusUpdate in JobExecutionContextExtensions
}
