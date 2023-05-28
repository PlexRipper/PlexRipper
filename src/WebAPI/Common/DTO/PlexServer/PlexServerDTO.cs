namespace PlexRipper.WebAPI.Common.DTO;

public class PlexServerDTO
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int OwnerId { get; set; }

    public string PlexServerOwnerUsername { get; set; }

    public string Device { get; set; }

    public string Platform { get; set; }

    public string PlatformVersion { get; set; }

    public string Product { get; set; }

    public string ProductVersion { get; set; }

    public string Provides { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastSeenAt { get; set; }

    public string MachineIdentifier { get; set; }

    public string PublicAddress { get; set; }

    public int PreferredConnectionId { get; set; }

    public bool Owned { get; set; }

    public bool Home { get; set; }

    public bool Synced { get; set; }

    public bool Relay { get; set; }

    public bool Presence { get; set; }

    public bool HttpsRequired { get; set; }

    public bool PublicAddressMatches { get; set; }

    public bool DnsRebindingProtection { get; set; }

    public bool NatLoopbackSupported { get; set; }

    public List<PlexServerConnectionDTO> PlexServerConnections { get; set; } = new();
}