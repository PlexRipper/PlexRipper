namespace Reaparr.Domain;

/// <summary>
/// The type of <see cref="PlexServerConnection"/>.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlexConnectionTypes
{
    [JsonStringEnumMemberName(nameof(Local))]
    Local = 0,

    [JsonStringEnumMemberName(nameof(Public))]
    Public = 1,

    [JsonStringEnumMemberName(nameof(PlexRelay))]
    PlexRelay = 2,

    [JsonStringEnumMemberName(nameof(Unknown))]
    Unknown = 3,
}
