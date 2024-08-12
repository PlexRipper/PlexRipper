namespace Application.Contracts;

public class PlexServerDTO
{
    public required int Id { get; set; }

    public required string Name { get; set; }

    public required long OwnerId { get; set; }

    public required string PlexServerOwnerUsername { get; set; }

    public required string Device { get; set; }

    public required string Platform { get; set; }

    public required string PlatformVersion { get; set; }

    public required string Product { get; set; }

    public required string ProductVersion { get; set; }

    public required string Provides { get; set; }

    public required DateTime CreatedAt { get; set; }

    public required DateTime LastSeenAt { get; set; }

    public required string MachineIdentifier { get; set; }

    public required string PublicAddress { get; set; }

    public required int PreferredConnectionId { get; set; }

    public required bool Owned { get; set; }

    public required bool Home { get; set; }

    public required bool IsEnabled { get; set; }

    public required bool Synced { get; set; }

    public required bool Relay { get; set; }

    public required bool Presence { get; set; }

    public required bool HttpsRequired { get; set; }

    public required bool PublicAddressMatches { get; set; }

    public required bool DnsRebindingProtection { get; set; }

    public required bool NatLoopbackSupported { get; set; }

    public required bool ServerFixApplyDNSFix { get; set; }
}
