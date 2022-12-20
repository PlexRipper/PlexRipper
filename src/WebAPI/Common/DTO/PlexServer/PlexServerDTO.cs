using Newtonsoft.Json;
using PlexRipper.DownloadManager;

namespace PlexRipper.WebAPI.Common.DTO;

public class PlexServerDTO
{
    [JsonProperty("id", Required = Required.Always)]
    public int Id { get; set; }

    [JsonProperty("name", Required = Required.Always)]
    public string Name { get; set; }

    [JsonProperty("ownerId", Required = Required.Always)]
    public int OwnerId { get; set; }

    [JsonProperty("plexServerOwnerUsername", Required = Required.Always)]
    public string PlexServerOwnerUsername { get; set; }

    [JsonProperty("device", Required = Required.Always)]
    public string Device { get; set; }

    [JsonProperty("platform", Required = Required.Always)]
    public string Platform { get; set; }

    [JsonProperty("platformVersion", Required = Required.Always)]
    public string PlatformVersion { get; set; }

    [JsonProperty("product", Required = Required.Always)]
    public string Product { get; set; }

    [JsonProperty("productVersion", Required = Required.Always)]
    public string ProductVersion { get; set; }

    [JsonProperty("provides", Required = Required.Always)]
    public string Provides { get; set; }

    [JsonProperty("createdAt", Required = Required.Always)]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("lastSeenAt", Required = Required.Always)]
    public DateTime LastSeenAt { get; set; }

    [JsonProperty("machineIdentifier", Required = Required.Always)]
    public string MachineIdentifier { get; set; }

    [JsonProperty("publicAddress", Required = Required.Always)]
    public string PublicAddress { get; set; }

    [JsonProperty("preferredConnectionId", Required = Required.Always)]
    public int PreferredConnectionId { get; set; }

    [JsonProperty("owned", Required = Required.Always)]
    public bool Owned { get; set; }

    [JsonProperty("home", Required = Required.Always)]
    public bool Home { get; set; }

    [JsonProperty("synced", Required = Required.Always)]
    public bool Synced { get; set; }

    [JsonProperty("relay", Required = Required.Always)]
    public bool Relay { get; set; }

    [JsonProperty("presence", Required = Required.Always)]
    public bool Presence { get; set; }

    [JsonProperty("httpsRequired", Required = Required.Always)]
    public bool HttpsRequired { get; set; }

    [JsonProperty("publicAddressMatches", Required = Required.Always)]
    public bool PublicAddressMatches { get; set; }

    [JsonProperty("dnsRebindingProtection", Required = Required.Always)]
    public bool DnsRebindingProtection { get; set; }

    [JsonProperty("natLoopbackSupported", Required = Required.Always)]
    public bool NatLoopbackSupported { get; set; }

    [JsonProperty("plexServerConnections", Required = Required.Always)]
    public List<PlexServerConnectionDTO> PlexServerConnections { get; set; } = new();

    [JsonProperty("downloadTasks", Required = Required.Always)]
    public List<DownloadProgressDTO> DownloadTasks { get; set; }
}