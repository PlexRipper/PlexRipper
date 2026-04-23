namespace Reaparr.Domain;

/// <summary>
/// Used to define the actions that can be taken on a <see cref="DownloadTaskGeneric"/>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DownloadActions
{
    [JsonStringEnumMemberName(nameof(Details))]
    Details = 0,

    [JsonStringEnumMemberName(nameof(Delete))]
    Delete = 1,

    [JsonStringEnumMemberName(nameof(Start))]
    Start = 2,

    [JsonStringEnumMemberName(nameof(Pause))]
    Pause = 3,

    [JsonStringEnumMemberName(nameof(Stop))]
    Stop = 4,

    [JsonStringEnumMemberName(nameof(Clear))]
    Clear = 5,

    [JsonStringEnumMemberName(nameof(Restart))]
    Restart = 6,
}
