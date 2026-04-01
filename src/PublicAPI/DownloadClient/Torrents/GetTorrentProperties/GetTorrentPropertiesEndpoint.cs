using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Reaparr.PublicAPI;

public record GetTorrentPropertiesRequest
{
    [QueryParam, BindFrom("hash")]
    public required string Hash { get; init; }
}

public sealed class GetTorrentPropertiesRequestValidator : Validator<GetTorrentPropertiesRequest>
{
    public GetTorrentPropertiesRequestValidator()
    {
        RuleFor(x => x.Hash).NotEmpty().WithMessage("Hash is required.");
    }
}

public record QBittorrentTorrentProperties
{
    [JsonPropertyName("save_path")]
    public string SavePath { get; set; } = string.Empty;

    [JsonPropertyName("seeding_time")]
    public long SeedingTime { get; set; }
}

public sealed class GetTorrentPropertiesEndpoint : Endpoint<GetTorrentPropertiesRequest, QBittorrentTorrentProperties>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly ILogger _log;

    public GetTorrentPropertiesEndpoint(ILogger logger, IReaparrDbContext dbContext)
    {
        _log = logger.ForContext<GetTorrentPropertiesEndpoint>();
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(PublicApiRoutes.DownloadClient + "/torrents/properties");
        Description(x => x.IsDownloadClient());
        AllowAnonymous();
        PreProcessor<DownloadClientAuthenticationPreProcessor<GetTorrentPropertiesRequest>>();
    }

    public override async Task HandleAsync(GetTorrentPropertiesRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        var file = await _dbContext
            .DownloadTaskTvShowEpisodeFile.Where(x => x.HashId == req.Hash)
            .Select(x => (DownloadTaskFileBase)x)
            .FirstOrDefaultAsync(ct);

        if (file is null)
        {
            file = await _dbContext
                .DownloadTaskMovieFile.Where(x => x.HashId == req.Hash)
                .Select(x => (DownloadTaskFileBase)x)
                .FirstOrDefaultAsync(ct);
        }

        if (file is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var savePath = !string.IsNullOrWhiteSpace(file.DownloadDirectory)
            ? file.DownloadDirectory
            : file.DestinationDirectory;

        var response = new QBittorrentTorrentProperties { SavePath = savePath, SeedingTime = 0 };

        await Send.OkAsync(response, ct);
    }
}
