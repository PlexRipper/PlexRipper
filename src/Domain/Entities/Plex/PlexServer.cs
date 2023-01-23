using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PlexRipper.Domain;

public class PlexServer : BaseEntity
{
    #region Properties

    /// <summary>
    /// Gets or sets the name of this <see cref="PlexServer"/>.
    /// </summary>
    [Column(Order = 1)]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the id Plex has assigned to the PlexAccount.
    /// </summary>
    [Column(Order = 2)]
    public long OwnerId { get; set; }

    /// <summary>
    /// Gets or sets what seems like the username of the Plex server owner.
    /// Mapped from "sourceTitle".
    /// </summary>
    [Column(Order = 3)]
    public string PlexServerOwnerUsername { get; set; }

    /// <summary>
    /// Gets or sets the type of hardware operating system this <see cref="PlexServer"/> is running.
    /// </summary>
    [Column(Order = 4)]
    public string Device { get; set; }

    /// <summary>
    /// Gets or sets the hardware operating system this <see cref="PlexServer"/> is running.
    /// </summary>
    [Column(Order = 5)]
    public string Platform { get; set; }

    /// <summary>
    /// Gets or sets the hardware operating system version this <see cref="PlexServer"/> is running.
    /// </summary>
    [Column(Order = 6)]
    public string PlatformVersion { get; set; }

    /// <summary>
    /// Gets or sets the Plex software this <see cref="PlexServer"/> is running.
    /// </summary>
    [Column(Order = 7)]
    public string Product { get; set; }

    /// <summary>
    /// Gets or sets the Plex software version this <see cref="PlexServer"/> is running.
    /// </summary>
    [Column(Order = 8)]
    public string ProductVersion { get; set; }

    /// <summary>
    /// Gets or sets the role this <see cref="PlexServer"/> provides, seems to be mostly "server".
    /// </summary>
    [Column(Order = 9)]
    public string Provides { get; set; }

    [Column(Order = 10)]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last time this server has been online based on what Plex has seen.
    /// </summary>
    [Column(Order = 11)]
    public DateTime LastSeenAt { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for this server. This is mapped from the new Plex clientId.
    /// </summary>
    [Column(Order = 12)]
    public string MachineIdentifier { get; set; }

    [Column(Order = 13)]
    public string PublicAddress { get; set; }

    [Column(Order = 14)]
    public int PreferredConnectionId { get; set; }

    [Column(Order = 15)]
    public bool Owned { get; set; }

    [Column(Order = 16)]
    public bool Home { get; set; }

    [Column(Order = 17)]
    public bool Synced { get; set; }

    [Column(Order = 18)]
    public bool Relay { get; set; }

    [Column(Order = 19)]
    public bool Presence { get; set; }

    [Column(Order = 20)]
    public bool HttpsRequired { get; set; }

    [Column(Order = 21)]
    public bool PublicAddressMatches { get; set; }

    [Column(Order = 22)]
    public bool DnsRebindingProtection { get; set; }

    [Column(Order = 23)]
    public bool NatLoopbackSupported { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether certain servers have protection or are misconfigured which is why we can apply certain fixes to facilitate server communication.
    /// This will attempt to connect on port 80 of the server.
    /// </summary>
    [Column(Order = 24)]
    public bool ServerFixApplyDNSFix { get; set; }

    #endregion

    #region Relationships

    public List<PlexAccountServer> PlexAccountServers { get; set; } = new();

    public List<PlexLibrary> PlexLibraries { get; set; } = new();

    public List<PlexServerStatus> ServerStatus { get; set; } = new();

    /// <summary>
    /// Gets or sets the different connections that can be used to communicate with the <see cref="PlexServer"/>.
    /// </summary>
    public List<PlexServerConnection> PlexServerConnections { get; set; } = new();

    #endregion

    #region Helpers

    /// <summary>
    /// Gets the library section url derived from the BaseUrl, e.g: http://112.202.10.213:32400/library/sections.
    /// </summary>
    [NotMapped]
    public string LibraryUrl => $"{GetServerUrl()}library/sections";

    /// <summary>
    /// Gets a value indicating whether this <see cref="PlexServer"/> has any DownloadTasks in any nested <see cref="PlexLibrary"/>.
    /// </summary>
    [NotMapped]
    public bool HasDownloadTasks => PlexLibraries?.Any(x => x.DownloadTasks?.Any() ?? false) ?? false;

    /// <summary>
    /// Gets a collection of all <see cref="DownloadTasks"/> included in the nested <see cref="PlexLibrary">PlexLibraries</see>.
    /// </summary>
    [NotMapped]
    public List<DownloadTask> DownloadTasks => PlexLibraries?.SelectMany(x => x.DownloadTasks).ToList() ?? new List<DownloadTask>();

    /// <summary>
    /// Gets the last known server status.
    /// </summary>
    [NotMapped]
    public PlexServerStatus Status
    {
        get
        {
            if (ServerStatus.Any())
                return ServerStatus.Last();

            // TODO Add initial server status when server is added to DB. Meaning there is always one.
            return new PlexServerStatus
            {
                Id = 0,
                IsSuccessful = false,
                PlexServer = this,
                StatusMessage = "Not checked yet",
                PlexServerId = Id,
                StatusCode = 0,
            };
        }
    }

    #endregion

    #region Operators

    public static bool operator ==(PlexServer left, PlexServer right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(PlexServer left, PlexServer right)
    {
        return !Equals(left, right);
    }

    #endregion

    /// <summary>
    /// Gets the server url based on the available connections, e.g: http://112.202.10.213:32400.
    /// </summary>
    /// <param name="plexServerConnectionId">The optional <see cref="PlexServerConnections"/> to use.</param>
    /// <returns>The connection url based on preference or on fallback.</returns>
    public string GetServerUrl(int plexServerConnectionId = 0)
    {
        if (!PlexServerConnections.Any())
            throw new Exception($"PlexServer with id {Id} and name {Name} has no connections available!");

        if (plexServerConnectionId > 0)
        {
            var connection = PlexServerConnections.Find(x => x.Id == plexServerConnectionId);
            if (connection is not null)
                return connection.Url;

            Log.Warning($"Could not find parameter {nameof(plexServerConnectionId)} with id {plexServerConnectionId} for server {Name}");
        }

        if (PreferredConnectionId > 0)
        {
            var connection = PlexServerConnections.Find(x => x.Id == PreferredConnectionId);
            if (connection is not null)
                return connection.Url;

            Log.Warning($"Could not find preferred connection with id {PreferredConnectionId} for server {Name}");
        }
        else
        {
            var connection = PlexServerConnections.Find(x => x.Address == PublicAddress);
            if (connection is not null)
                return connection.Url;
        }

        Log.Warning($"Could not find connection based on public address: {PublicAddress} for server {Name}");
        Log.Warning($"Trying the first connection: {PlexServerConnections.First().Url}");
        return PlexServerConnections.First().Url;
    }

    #region Equality

    /// <inheritdoc/>
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = "Cannot be ReadOnly due to usage in Entity Framework as Entities")]
    public override int GetHashCode()
    {
        var hashCode = default(HashCode);
        hashCode.Add(Name);
        hashCode.Add(OwnerId);
        hashCode.Add(PlexServerOwnerUsername);
        hashCode.Add(Device);
        hashCode.Add(Platform);
        hashCode.Add(PlatformVersion);
        hashCode.Add(Product);
        hashCode.Add(ProductVersion);
        hashCode.Add(Provides);
        hashCode.Add(CreatedAt);
        hashCode.Add(LastSeenAt);
        hashCode.Add(MachineIdentifier);
        hashCode.Add(PublicAddress);
        hashCode.Add(PreferredConnectionId);
        hashCode.Add(Owned);
        hashCode.Add(Home);
        hashCode.Add(Synced);
        hashCode.Add(Relay);
        hashCode.Add(Presence);
        hashCode.Add(HttpsRequired);
        hashCode.Add(PublicAddressMatches);
        hashCode.Add(DnsRebindingProtection);
        hashCode.Add(NatLoopbackSupported);
        hashCode.Add(ServerFixApplyDNSFix);
        return hashCode.ToHashCode();
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;

        return Equals((PlexServer)obj);
    }

    /// <summary>
    /// Compares this <see cref="PlexServer"/> against another <see cref="PlexServer"/> without comparing the Id and navigation properties.
    /// </summary>
    /// <param name="other">The other <see cref="PlexServer"/> to compare against.</param>
    /// <returns>Returns whether these <see cref="PlexServer">PlexServers</see> are equal.</returns>
    protected bool Equals(PlexServer other)
    {
        return Name == other.Name && OwnerId == other.OwnerId && PlexServerOwnerUsername == other.PlexServerOwnerUsername && Device == other.Device &&
               Platform == other.Platform && PlatformVersion == other.PlatformVersion && Product == other.Product && ProductVersion == other.ProductVersion &&
               Provides == other.Provides && CreatedAt.Equals(other.CreatedAt) && LastSeenAt.Equals(other.LastSeenAt) &&
               MachineIdentifier == other.MachineIdentifier && PublicAddress == other.PublicAddress && PreferredConnectionId == other.PreferredConnectionId &&
               Owned == other.Owned && Home == other.Home && Synced == other.Synced && Relay == other.Relay && Presence == other.Presence &&
               HttpsRequired == other.HttpsRequired && PublicAddressMatches == other.PublicAddressMatches &&
               DnsRebindingProtection == other.DnsRebindingProtection && NatLoopbackSupported == other.NatLoopbackSupported &&
               ServerFixApplyDNSFix == other.ServerFixApplyDNSFix;
    }

    #endregion
}