namespace PlexRipper.PlexApi.Api;

// Used for retrieving servers by plex account token
// e.g: https://plex.tv/api/v2/resources?X-Plex-Token=XXXXXXXXXXXX&X-Plex-Client-Identifier=chrome
// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);

public class ServerResource
{
    public string Name { get; set; }

    public string Product { get; set; }

    public string ProductVersion { get; set; }

    public string Platform { get; set; }

    public string PlatformVersion { get; set; }

    public string Device { get; set; }

    public string ClientIdentifier { get; set; }

    public string CreatedAt { get; set; }

    public string LastSeenAt { get; set; }

    public string Provides { get; set; }

    public int? OwnerId { get; set; }

    public string SourceTitle { get; set; }

    public string PublicAddress { get; set; }

    public string AccessToken { get; set; }

    public bool Owned { get; set; }

    public bool Home { get; set; }

    public bool Synced { get; set; }

    public bool Relay { get; set; }

    public bool Presence { get; set; }

    public bool HttpsRequired { get; set; }

    public bool PublicAddressMatches { get; set; }

    public bool DnsRebindingProtection { get; set; }

    public bool NatLoopbackSupported { get; set; }

    public List<ServerResourceConnection> Connections { get; set; }
}

public class ServerResourceConnection
{
    public string Protocol { get; set; }

    public string Address { get; set; }

    public int Port { get; set; }

    public string Uri { get; set; }

    public bool Local { get; set; }

    public bool Relay { get; set; }

    public bool IPv6 { get; set; }
}
