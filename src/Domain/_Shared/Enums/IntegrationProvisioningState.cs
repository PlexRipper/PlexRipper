namespace Reaparr.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IntegrationProvisioningState
{
    [JsonStringEnumMemberName(nameof(Unconfigured))]
    Unconfigured = 0,

    [JsonStringEnumMemberName(nameof(ChangesPending))]
    ChangesPending = 1,

    [JsonStringEnumMemberName(nameof(Provisioning))]
    Provisioning = 2,

    [JsonStringEnumMemberName(nameof(Configured))]
    Configured = 3,

    [JsonStringEnumMemberName(nameof(Error))]
    Error = 4,

    [JsonStringEnumMemberName(nameof(Deleting))]
    Deleting = 5,
}
