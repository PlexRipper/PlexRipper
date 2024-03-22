using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace PlexRipper.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationLevel
{
    // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc and that there is no skip in between
    // Otherwise the Typescript DTO translator in the front-end starts messing up
    [EnumMember(Value = "None")]
    None = 0,

    [EnumMember(Value = "Verbose")]
    Verbose = 1,

    [EnumMember(Value = "Debug")]
    Debug = 2,

    [EnumMember(Value = "Information")]
    Information = 3,

    [EnumMember(Value = "Success")]
    Success = 4,

    [EnumMember(Value = "Warning")]
    Warning = 5,

    [EnumMember(Value = "Error")]
    Error = 6,

    [EnumMember(Value = "Fatal")]
    Fatal = 7,
}