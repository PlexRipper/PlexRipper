using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace PlexRipper.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DownloadTaskPhase
{
    // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc and that there is no skip in between
    // Otherwise the TypeScript DTO translator in the front-end starts messing up
    [EnumMember(Value = "None")]
    None = 0,

    [EnumMember(Value = "Downloading")]
    Downloading = 1,

    [EnumMember(Value = "FileTransfer")]
    FileTransfer = 2,

    [EnumMember(Value = "Completed")]
    Completed = 3,

    [EnumMember(Value = "Unknown")]
    Unknown = 4,
}
