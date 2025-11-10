using System.Text;
using FastEndpoints;
using Reaparr.Data.Contracts;

namespace Reaparr.PublicAPI;

public sealed class GetTorrentFileEndpoint : EndpointWithoutRequest
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public GetTorrentFileEndpoint(ILogger logger, IReaparrDbContext dbContext)
    {
        _log = logger.ForContext<GetTorrentFileEndpoint>();
        _dbContext = dbContext;
    }

    
    public override void Configure()
    {
        Get(PublicApiRoutes.DownloadClient + "/torrents/file/{hash}.torrent");
        Description(x => x.IsDownloadClient());
        AllowAnonymous();
        PreProcessor<DownloadClientAuthenticationPreProcessor<EmptyRequest>>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext);
        
        _log.Warning("GetTorrentFileEndpoint called but not implemented.");
        
        var bytes = Encoding.UTF8.GetBytes("d8:announce0:e");
        HttpContext.Response.ContentType = "application/x-bittorrent";
        await HttpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length, ct);
    }
}