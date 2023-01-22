using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Api;

// Used for retrieving servers by plex account token
// e.g: https://plex.tv/api/v2/resources?X-Plex-Token=XXXXXXXXXXXX&X-Plex-Client-Identifier=chrome
// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);

public class ServerResource
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("product")]
    public string Product { get; set; }

    [JsonPropertyName("productVersion")]
    public string ProductVersion { get; set; }

    [JsonPropertyName("platform")]
    public string Platform { get; set; }

    [JsonPropertyName("platformVersion")]
    public string PlatformVersion { get; set; }

    [JsonPropertyName("device")]
    public string Device { get; set; }

    [JsonPropertyName("clientIdentifier")]
    public string ClientIdentifier { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("lastSeenAt")]
    public DateTime LastSeenAt { get; set; }

    [JsonPropertyName("provides")]
    public string Provides { get; set; }

    [JsonPropertyName("ownerId")]
    public int? OwnerId { get; set; }

    [JsonPropertyName("sourceTitle")]
    public string SourceTitle { get; set; }

    [JsonPropertyName("publicAddress")]
    public string PublicAddress { get; set; }

    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; }

    [JsonPropertyName("owned")]
    public bool Owned { get; set; }

    [JsonPropertyName("home")]
    public bool Home { get; set; }

    [JsonPropertyName("synced")]
    public bool Synced { get; set; }

    [JsonPropertyName("relay")]
    public bool Relay { get; set; }

    [JsonPropertyName("presence")]
    public bool Presence { get; set; }

    [JsonPropertyName("httpsRequired")]
    public bool HttpsRequired { get; set; }

    [JsonPropertyName("publicAddressMatches")]
    public bool PublicAddressMatches { get; set; }

    [JsonPropertyName("dnsRebindingProtection")]
    public bool DnsRebindingProtection { get; set; }

    [JsonPropertyName("natLoopbackSupported")]
    public bool NatLoopbackSupported { get; set; }

    [JsonPropertyName("connections")]
    public List<ServerResourceConnection> Connections { get; set; }
}

public class ServerResourceConnection
{
    [JsonPropertyName("protocol")]
    public string Protocol { get; set; }

    [JsonPropertyName("address")]
    public string Address { get; set; }

    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("uri")]
    public string Uri { get; set; }

    [JsonPropertyName("local")]
    public bool Local { get; set; }

    [JsonPropertyName("relay")]
    public bool Relay { get; set; }

    [JsonPropertyName("IPv6")]
    public bool IPv6 { get; set; }
}