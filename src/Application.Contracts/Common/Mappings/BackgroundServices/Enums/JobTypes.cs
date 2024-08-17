using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Application.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum JobTypes
{
    // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc and that there is no skip in between
    // Otherwise the Typescript DTO translator in the front-end starts messing up
    [EnumMember(Value = "Unknown")]
    Unknown = 0,

    [EnumMember(Value = nameof(CheckPlexServerConnectionsJob))]
    CheckPlexServerConnectionsJob = 1,

    [EnumMember(Value = nameof(DownloadJob))]
    DownloadJob = 2,

    [EnumMember(Value = nameof(DownloadProgressJob))]
    DownloadProgressJob = 3,

    [EnumMember(Value = nameof(SyncServerJob))]
    SyncServerJob = 4,

    [EnumMember(Value = nameof(DownloadProgressJobs))]
    DownloadProgressJobs = 5,

    [EnumMember(Value = nameof(InspectPlexServerJob))]
    InspectPlexServerJob = 6,

    [EnumMember(Value = nameof(FileMergeJob))]
    FileMergeJob = 7,
}
