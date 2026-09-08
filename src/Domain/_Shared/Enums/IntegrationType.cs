namespace Reaparr.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IntegrationType
{
    [JsonStringEnumMemberName(nameof(Sonarr))]
    Sonarr = 0,

    [JsonStringEnumMemberName(nameof(Radarr))]
    Radarr = 1,
}
