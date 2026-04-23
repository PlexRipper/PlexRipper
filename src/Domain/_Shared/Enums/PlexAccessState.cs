namespace Reaparr.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlexAccessState
{
    [JsonStringEnumMemberName(nameof(Unknown))]
    Unknown = 0,

    [JsonStringEnumMemberName(nameof(Revoked))]
    Revoked = 1,

    [JsonStringEnumMemberName(nameof(Updated))]
    Updated = 2,

    [JsonStringEnumMemberName(nameof(Granted))]
    Granted = 3,
}
