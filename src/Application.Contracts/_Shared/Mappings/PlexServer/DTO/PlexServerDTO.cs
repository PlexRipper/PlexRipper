namespace Reaparr.Application.Contracts;

public class PlexServerDTO
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required long OwnerId { get; init; }

    public required string PlexServerOwnerUsername { get; init; }

    public required string Device { get; init; }

    public required string Platform { get; init; }

    public required string PlatformVersion { get; init; }

    public required string Product { get; init; }

    public required string ProductVersion { get; init; }

    public required string Provides { get; init; }

    public required DateTime CreatedAt { get; init; }

    public required DateTime LastSeenAt { get; init; }

    public required string MachineIdentifier { get; init; }

    public required string PublicAddress { get; init; }

    public required int PreferredConnectionId { get; init; }

    public required bool Owned { get; init; }

    public required bool Home { get; init; }

    public required bool IsEnabled { get; init; }

    public required bool Synced { get; init; }

    public required bool Relay { get; init; }

    public required bool Presence { get; init; }

    public required bool HttpsRequired { get; init; }

    public required bool PublicAddressMatches { get; init; }

    public required bool DnsRebindingProtection { get; init; }

    public required bool NatLoopbackSupported { get; init; }

    public required bool IsDownloadsPausedByUser { get; init; }
}
