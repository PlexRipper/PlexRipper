using System.Text.Json.Serialization;

namespace Reaparr.Logging;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LogSeverity
{
    // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc. and that there is no skip in between
    // Otherwise the TypeScript DTO translator in the front-end starts messing up
    [JsonStringEnumMemberName(nameof(None))]
    None = 0,

    [JsonStringEnumMemberName(nameof(Verbose))]
    Verbose = 1,

    [JsonStringEnumMemberName(nameof(Debug))]
    Debug = 2,

    [JsonStringEnumMemberName(nameof(Information))]
    Information = 3,

    [JsonStringEnumMemberName(nameof(Success))]
    Success = 4,

    [JsonStringEnumMemberName(nameof(Warning))]
    Warning = 5,

    [JsonStringEnumMemberName(nameof(Error))]
    Error = 6,

    [JsonStringEnumMemberName(nameof(Fatal))]
    Fatal = 7,
}
