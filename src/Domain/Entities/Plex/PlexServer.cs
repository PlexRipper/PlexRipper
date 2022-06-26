using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class PlexServer : BaseEntity
{
    [Column(Order = 1)]
    public string Name { get; set; }

    [Column(Order = 2)]
    public string Scheme { get; set; }

    [Column(Order = 3)]
    public string Address { get; set; }

    [Column(Order = 4)]
    public int Port { get; set; }

    [Column(Order = 5)]
    public string Version { get; set; }

    [Column(Order = 6)]
    public string Host { get; set; }

    [Column(Order = 7)]
    public string LocalAddresses { get; set; }

    [Column(Order = 8)]
    public string MachineIdentifier { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public long OwnerId { get; set; }

    /// <summary>
    /// Certain servers have protection or are misconfigured which is why we can apply certain fixes to facilitate server communication.
    /// This will attempt to connect on port 80 of the server.
    /// </summary>
    public bool ServerFixApplyDNSFix { get; set; }

    #region Relationships

    public List<PlexAccountServer> PlexAccountServers { get; set; } = new();

    public List<PlexLibrary> PlexLibraries { get; set; } = new();

    public List<PlexServerStatus> ServerStatus { get; set; } = new();

    #endregion

    #region Helpers

    /// <summary>
    /// Gets the server url, e.g: http://112.202.10.213:32400.
    /// </summary>
    [NotMapped]
    public string ServerUrl
    {
        get
        {
            switch (Scheme)
            {
                case "http":
                    return $"{Scheme}://{Address}:{(ServerFixApplyDNSFix ? 80 : Port)}";
                case "https":
                    return $"{Scheme}://{Address}:{(ServerFixApplyDNSFix ? 443 : Port)}";
                default:
                    return $"{Scheme}://{Address}:{Port}";
            }
        }
    }

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
            {
                return ServerStatus.Last();
            }

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