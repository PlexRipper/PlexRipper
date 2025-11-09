using System.Text.Json.Serialization;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Reaparr.Data.Contracts;

namespace Reaparr.PublicAPI;

public record TorrentsInfoEndpointRequest
{
    [QueryParam, BindFrom("category")]
    public required string Category { get; init; }
}

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
    public decimal Progress { get; set; }

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

public sealed class TorrentsInfoEndpoint : Endpoint<TorrentsInfoEndpointRequest, List<QBittorrentTorrentInfo>>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly ILogger _log;

    public TorrentsInfoEndpoint(ILogger logger, IReaparrDbContext dbContext)
    {
        _log = logger.ForContext<TorrentsInfoEndpoint>();
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(PublicApiRoutes.DownloadClient + "/torrents/info");
        Description(x => x.IsDownloadClient());
        AllowAnonymous();
        PreProcessor<DownloadClientAuthenticationPreProcessor<TorrentsInfoEndpointRequest>>();
    }

    public override async Task HandleAsync(TorrentsInfoEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext);

        // Query all download tasks that have a HashId (Sonarr/Radarr tracking id)
        var episodeFilesTask = _dbContext
            .DownloadTaskTvShowEpisodeFile
            .Where(x => x.HashId != null)
            .Include(x => x.Parent)
            .ToListAsync(ct);

        var movieFilesTask = _dbContext
            .DownloadTaskMovieFile
            .Where(x => x.HashId != null)
            .Include(x => x.Parent)
            .ToListAsync(ct);

        await Task.WhenAll(episodeFilesTask, movieFilesTask);

        var episodeInfos = episodeFilesTask.Result.Select(MapToTorrentInfo).ToList();
        var movieInfos = movieFilesTask.Result.Select(MapToTorrentInfo).ToList();

        var torrents = new List<QBittorrentTorrentInfo>(episodeInfos.Count + movieInfos.Count);
        torrents.AddRange(episodeInfos);
        torrents.AddRange(movieInfos);

        await Send.OkAsync(torrents, ct);
    }

    private static QBittorrentTorrentInfo MapToTorrentInfo(DownloadTaskFileBase file)
    {
        // Save path: prefer active download directory, else destination directory
        var savePath = !string.IsNullOrWhiteSpace(file.DownloadDirectory)
            ? file.DownloadDirectory
            : (file.DestinationDirectory);

        return new QBittorrentTorrentInfo
        {
            Hash = file.HashId!,
            Name = file.FileName,
            Size = file.DataTotal,
            Progress = file.Percentage,
            DlSpeed = file.Speed,
            Eta = (int)file.TimeRemaining,
            State = MapStatusToQbittorrentState(file.DownloadStatus),
            SavePath = savePath,
        };
    }

    private static string MapStatusToQbittorrentState(DownloadStatus status) => status switch
    {
        DownloadStatus.Downloading or DownloadStatus.DownloadFinished => "downloading",
        DownloadStatus.Paused or DownloadStatus.MovePaused => "pausedDL",
        DownloadStatus.Queued => "queuedDL",
        DownloadStatus.Completed => "stalledUP",
        DownloadStatus.Error or DownloadStatus.MoveError => "error",
        _ => "downloading",
    };
}