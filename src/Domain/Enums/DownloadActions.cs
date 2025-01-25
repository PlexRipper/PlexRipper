using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace PlexRipper.Domain;

/// <summary>
/// Used to define the actions that can be taken on a <see cref="DownloadTaskGeneric"/>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DownloadActions
{
    [EnumMember(Value = nameof(Details))]
    Details = 0,

    [EnumMember(Value = nameof(Delete))]
    Delete = 1,

    [EnumMember(Value = nameof(Start))]
    Start = 2,

    [EnumMember(Value = nameof(Pause))]
    Pause = 3,

    [EnumMember(Value = nameof(Stop))]
    Stop = 4,

    [EnumMember(Value = nameof(Clear))]
    Clear = 5,

    [EnumMember(Value = nameof(Restart))]
    Restart = 6,
}
