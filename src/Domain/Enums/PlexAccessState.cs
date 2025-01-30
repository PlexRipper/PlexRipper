using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace PlexRipper.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlexAccessState
{
    [EnumMember(Value = nameof(Revoked))]
    Revoked = 0,

    [EnumMember(Value = nameof(Updated))]
    Updated = 1,

    [EnumMember(Value = nameof(Granted))]
    Granted = 2,
}
