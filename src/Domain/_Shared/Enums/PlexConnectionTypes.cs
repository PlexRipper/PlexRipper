using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Reaparr.Domain;

/// <summary>
/// The type of <see cref="PlexServerConnection"/>.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlexConnectionTypes
{
    [EnumMember(Value = nameof(Local))]
    Local = 0,

    [EnumMember(Value = nameof(Public))]
    Public = 1,

    [EnumMember(Value = nameof(PlexRelay))]
    PlexRelay = 2,

    [EnumMember(Value = nameof(Unknown))]
    Unknown = 3,
}
