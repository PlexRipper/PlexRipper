namespace Reaparr.Application.Contracts;

public static class PlexServerMapper
{
    #region ToDTO

    public static PlexServerDTO ToDTO(this PlexServer source) =>
        new()
        {
            Id = source.Id,
            Name = source.Name,
            OwnerId = source.OwnerId,
            PlexServerOwnerUsername = source.PlexServerOwnerUsername,
            Device = source.Device,
            Platform = source.Platform,
            PlatformVersion = source.PlatformVersion,
            Product = source.Product,
            ProductVersion = source.ProductVersion,
            Provides = source.Provides,
            CreatedAt = source.CreatedAt,
            LastSeenAt = source.LastSeenAt,
            MachineIdentifier = source.MachineIdentifier,
            PublicAddress = source.PublicAddress,
            PreferredConnectionId = source.PreferredConnectionId,
            IsEnabled = source.IsEnabled,
            Owned = source.Owned,
            Synced = source.Synced,
            Relay = source.Relay,
            Presence = source.Presence,
            HttpsRequired = source.HttpsRequired,
            PublicAddressMatches = source.PublicAddressMatches,
            DnsRebindingProtection = source.DnsRebindingProtection,
            NatLoopbackSupported = source.NatLoopbackSupported,
            IsDownloadsPausedByUser = source.IsDownloadsPausedByUser,
        };

    public static List<PlexServerDTO> ToDTO(this List<PlexServer> source) => source.ConvertAll(ToDTO);

    #endregion

    #region ToModel

    public static PlexServer ToModel(this PlexServerDTO source) =>
        new()
        {
            Id = source.Id,
            Name = source.Name,
            OwnerId = source.OwnerId,
            PlexServerOwnerUsername = source.PlexServerOwnerUsername,
            Device = source.Device,
            Platform = source.Platform,
            PlatformVersion = source.PlatformVersion,
            Product = source.Product,
            ProductVersion = source.ProductVersion,
            Provides = source.Provides,
            CreatedAt = source.CreatedAt,
            LastSeenAt = source.LastSeenAt,
            MachineIdentifier = source.MachineIdentifier,
            PublicAddress = source.PublicAddress,
            PreferredConnectionId = source.PreferredConnectionId,
            IsEnabled = source.IsEnabled,
            IsDownloadsPausedByUser = source.IsDownloadsPausedByUser,
            OwnedOverride = source.Owned,
            Synced = source.Synced,
            Relay = source.Relay,
            Presence = source.Presence,
            HttpsRequired = source.HttpsRequired,
            PublicAddressMatches = source.PublicAddressMatches,
            DnsRebindingProtection = source.DnsRebindingProtection,
            NatLoopbackSupported = source.NatLoopbackSupported,
            PlexAccountServers = [],
            PlexLibraries = [],
            ServerStatus = [],
            PlexServerConnections = [],
        };

    public static List<PlexServer> ToModel(this List<PlexServerDTO> source) => source.ConvertAll(ToModel);

    #endregion
}
