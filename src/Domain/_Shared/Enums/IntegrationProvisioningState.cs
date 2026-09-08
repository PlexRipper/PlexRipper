namespace Reaparr.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IntegrationProvisioningState
{
    [JsonStringEnumMemberName(nameof(Unconfigured))]
    Unconfigured = 0,

    [JsonStringEnumMemberName(nameof(ChangesPending))]
    ChangesPending = 1,

    [JsonStringEnumMemberName(nameof(Configured))]
    Configured = 2,
}
