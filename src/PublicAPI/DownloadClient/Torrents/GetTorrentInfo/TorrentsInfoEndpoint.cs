using System.Text.Json.Serialization;
using FastEndpoints;

namespace Reaparr.PublicAPI;

public record QBittorrentTorrentInfo
{
    /// <summary>
    /// SHA-1 hash string of the torrent (40 hex characters). Unique identifier.
    /// </summary>
    [JsonPropertyName("hash")]
    public string Hash { get; set; } = string.Empty;

    /// <summary>
    /// Torrent name (usually the root folder or main file).
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Total size of the torrent in bytes.
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; set; }

    /// <summary>
    /// Download progress (0.0 = 0%, 1.0 = 100%).
    /// </summary>
    [JsonPropertyName("progress")]
    public double Progress { get; set; }

    /// <summary>
    /// Current download speed in bytes per second.
    /// </summary>
    [JsonPropertyName("dlspeed")]
    public long DlSpeed { get; set; }

    /// <summary>
    /// Estimated time of arrival (time left) in seconds. -1 if unknown.
    /// </summary>
    [JsonPropertyName("eta")]
    public int Eta { get; set; }

    /// <summary>
    /// Current torrent state (e.g. "downloading", "pausedDL", "queuedDL", "stalledDL").
    /// </summary>
    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Absolute path on disk where torrent data is being saved.
    /// </summary>
    [JsonPropertyName("save_path")]
    public string SavePath { get; set; } = string.Empty;
}

public sealed class TorrentsInfoEndpoint : EndpointWithoutRequest<List<QBittorrentTorrentInfo>>
{
    public override void Configure()
    {
        Get(PublicApiRoutes.DownloadClient + "/torrents/info");
        Description(x => x.IsDownloadClient());
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var torrents = new List<QBittorrentTorrentInfo>
        {
            new QBittorrentTorrentInfo
            {
                Hash = "abc123def4567890abc123def4567890abc12345",
                Name = "Ubuntu.22.04.3.Desktop.amd64.iso",
                Size = 4700372992, // ~4.4 GB
                Progress = 0.75,
                DlSpeed = 2097152, // 2 MB/s
                Eta = 900, // 15 minutes
                State = "downloading",
                SavePath = "/home/user/Downloads"
            },
            new QBittorrentTorrentInfo
            {
                Hash = "def456abc7890123def456abc7890123def456ab",
                Name = "Big.Buck.Bunny.1080p.mp4",
                Size = 1073741824, // 1 GB
                Progress = 1.0,
                DlSpeed = 0,
                Eta = 8640000, // effectively infinity (completed)
                State = "stalledUP",
                SavePath = "/home/user/Downloads/Movies"
            }
        };

        await Send.OkAsync(torrents, ct);
    }
}