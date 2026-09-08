namespace Reaparr.Domain;

public class SonarrIntegration : BaseEntityGuid
{
    /// <summary>
    /// Gets or sets the user-facing name of the Sonarr integration.
    /// </summary>
    [Column(Order = 1)]
    public required string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the base URL of the Sonarr instance.
    /// </summary>
    [Column(Order = 2)]
    public required string BaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the API key used to authenticate requests to Sonarr.
    /// </summary>
    [Column(Order = 3)]
    public required string SonarrApiKey { get; set; }

    /// <summary>
    /// Gets or sets the API key used by Sonarr to authenticate qBittorrent-compatible requests to Reaparr.
    /// </summary>
    [Column(Order = 4)]
    public required string QBittorrentApiKey { get; set; }

    /// <summary>
    /// Gets or sets the API key used by Sonarr to authenticate Torznab requests to Reaparr.
    /// </summary>
    [Column(Order = 5)]
    public required string TorznabApiKey { get; set; }

    /// <summary>
    /// Gets or sets the Sonarr download category owned by this integration.
    /// </summary>
    [Column(Order = 6)]
    public required string Category { get; set; }

    /// <summary>
    /// Gets or sets the Sonarr identifier of the configured Reaparr download client.
    /// </summary>
    [Column(Order = 7)]
    public int? ExternalDownloadClientId { get; set; }

    /// <summary>
    /// Gets or sets the Sonarr identifier of the configured Reaparr indexer.
    /// </summary>
    [Column(Order = 8)]
    public int? ExternalIndexerId { get; set; }

    /// <summary>
    /// Gets or sets the durable provisioning state of this integration.
    /// </summary>
    [Column(Order = 9)]
    public required IntegrationProvisioningState ProvisioningState { get; set; }

    /// <summary>
    /// Gets or sets the outcome of the latest completed connection test.
    /// </summary>
    [Column(Order = 10)]
    public TestConnectionStatus LastConnectionTestStatus { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code returned by Sonarr during the latest completed connection test.
    /// </summary>
    [Column(Order = 11)]
    public int? LastConnectionTestHttpStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the error returned by the latest completed connection test.
    /// </summary>
    [Column(Order = 12)]
    public string? LastConnectionTestErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets when the latest completed connection test finished in UTC.
    /// </summary>
    [Column(Order = 13)]
    public DateTime? LastConnectionTestedAt { get; set; }

    /// <summary>
    /// Gets or sets the required Reaparr download folder used by this integration.
    /// </summary>
    [Column(Order = 14)]
    public required int DownloadFolderId { get; set; }

    /// <summary>
    /// Gets or sets the download folder used by this integration.
    /// </summary>
    public FolderPath? DownloadFolder { get; set; }

    /// <summary>
    /// Gets or sets the download tasks owned by this integration.
    /// </summary>
    public ICollection<DownloadTaskBase> DownloadTasks { get; set; } = [];
}
