using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Reaparr.Domain;

/// <summary>
/// Used to define the type of data being sent.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RefreshDataType
{
    [EnumMember(Value = nameof(PlexAccount))]
    PlexAccount = 0,

    [EnumMember(Value = nameof(PlexServer))]
    PlexServer = 1,

    [EnumMember(Value = nameof(PlexLibrary))]
    PlexLibrary = 2,

    [EnumMember(Value = nameof(PlexServerConnection))]
    PlexServerConnection = 3,
}
