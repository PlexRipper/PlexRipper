using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Reaparr.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlexAccessState
{
    [EnumMember(Value = nameof(Unknown))]
    Unknown = 0,

    [EnumMember(Value = nameof(Revoked))]
    Revoked = 1,

    [EnumMember(Value = nameof(Updated))]
    Updated = 2,

    [EnumMember(Value = nameof(Granted))]
    Granted = 3,
}
