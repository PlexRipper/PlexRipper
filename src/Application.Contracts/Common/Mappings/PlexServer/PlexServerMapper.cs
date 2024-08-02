using PlexRipper.Domain;

namespace Application.Contracts;

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
            Owned = source.Owned,
            Home = source.Home,
            Synced = source.Synced,
            Relay = source.Relay,
            Presence = source.Presence,
            HttpsRequired = source.HttpsRequired,
            PublicAddressMatches = source.PublicAddressMatches,
            DnsRebindingProtection = source.DnsRebindingProtection,
            NatLoopbackSupported = source.NatLoopbackSupported,
            ServerFixApplyDNSFix = source.ServerFixApplyDNSFix,
            PlexServerConnections = source.PlexServerConnections.ToDTO(),
        };

    public static List<PlexServerDTO> ToDTO(this List<PlexServer> source) => source.ConvertAll(ToDTO);

    #endregion

    #region PlexServerAccess

    public static PlexServerAccessDTO ToAccessDTO(this PlexServer source) =>
        new() { PlexServerId = source.Id, PlexLibraryIds = source.PlexLibraries.Select(x => x.Id).ToList(), };

    public static List<PlexServerAccessDTO> ToAccessDTO(this List<PlexServer> source) => source.ConvertAll(ToAccessDTO);

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
            Owned = source.Owned,
            Home = source.Home,
            Synced = source.Synced,
            Relay = source.Relay,
            Presence = source.Presence,
            HttpsRequired = source.HttpsRequired,
            PublicAddressMatches = source.PublicAddressMatches,
            DnsRebindingProtection = source.DnsRebindingProtection,
            NatLoopbackSupported = source.NatLoopbackSupported,
            ServerFixApplyDNSFix = source.ServerFixApplyDNSFix,
            PlexAccountServers = new List<PlexAccountServer>(),
            PlexLibraries = new List<PlexLibrary>(),
            ServerStatus = new List<PlexServerStatus>(),
            PlexServerConnections = source.PlexServerConnections.ToModel(),
        };

    public static List<PlexServer> ToModel(this List<PlexServerDTO> source) => source.ConvertAll(ToModel);

    #endregion
}
