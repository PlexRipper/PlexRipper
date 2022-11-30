using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class PlexServer : BaseEntity
{
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


    [Column(Order = 4)]
    public string Device { get; set; }

    [Column(Order = 5)]
    public string Platform { get; set; }

    [Column(Order = 6)]
    public string PlatformVersion { get; set; }

    [Column(Order = 7)]
    public string Product { get; set; }

    [Column(Order = 8)]
    public string ProductVersion { get; set; }


    /// <summary>
    /// Gets or sets the role this <see cref="PlexServer"/> provides, seems to be mostly "server".
    /// </summary>
    [Column(Order = 9)]
    public string Provides { get; set; }

    [Column(Order = 10)]
    public DateTime CreatedAt { get; set; }

    [Column(Order = 11)]
    public DateTime LastSeenAt { get; set; }

    [Column(Order = 12)]
    public string MachineIdentifier { get; set; }

    [Column(Order = 13)]
    public string PublicAddress { get; set; }

    [Column(Order = 14)]
    public int PreferredConnection { get; set; }

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
    /// Certain servers have protection or are misconfigured which is why we can apply certain fixes to facilitate server communication.
    /// This will attempt to connect on port 80 of the server.
    /// </summary>
    [Column(Order = 24)]
    public bool ServerFixApplyDNSFix { get; set; }


    #region Relationships

    public List<PlexAccountServer> PlexAccountServers { get; set; } = new();

    public List<PlexLibrary> PlexLibraries { get; set; } = new();

    public List<PlexServerStatus> ServerStatus { get; set; } = new();

    public List<PlexServerConnection> PlexServerConnections { get; set; } = new();

    #endregion

    #region Helpers

    /// <summary>
    /// Gets the server url, e.g: http://112.202.10.213:32400.
    /// </summary>
    [NotMapped]
    public string ServerUrl => PlexServerConnections.First().Uri.ToString();

    /// <summary>
    /// Gets the library section url derived from the BaseUrl, e.g: http://112.202.10.213:32400/library/sections.
    /// </summary>
    [NotMapped]
    public string LibraryUrl => $"{ServerUrl}/library/sections";

    /// <summary>
    /// Gets or sets the temporary auth token.
    /// Do not use this property to retrieve the needed authToken, this is only meant to transfer the incoming authToken from the plexApi to the Database.
    /// See AddOrUpdatePlexServersHandler.
    /// </summary>
    [NotMapped]
    public string AccessToken { get; set; }

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
}