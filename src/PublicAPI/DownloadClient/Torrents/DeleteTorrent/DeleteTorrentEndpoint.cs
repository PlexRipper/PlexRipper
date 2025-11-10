using System.Text.Json;
using FastEndpoints;

namespace Reaparr.PublicAPI;

public sealed class DeleteTorrentRequest
{
	public List<string> Hashes { get; set; } = new();
}

public sealed class DeleteTorrentResponse
{
	public bool Success { get; set; }
}

public sealed class DeleteTorrentEndpoint : Endpoint<DeleteTorrentRequest, DeleteTorrentResponse>
{
    private readonly ILogger _log;

    public DeleteTorrentEndpoint(ILogger log)
    {
        _log = log.ForContext<DeleteTorrentEndpoint>();
    }
    
	public override void Configure()
	{
		Post(PublicApiRoutes.DownloadClient + "/torrents/delete");
        Description(x => x.IsDownloadClient());
        AllowAnonymous();
        PreProcessor<DownloadClientAuthenticationPreProcessor<DeleteTorrentRequest>>();
	}

	public override async Task HandleAsync(DeleteTorrentRequest req, CancellationToken ct)
	{
        _log.Here().DebugApiCall(HttpContext, req);
        
        _log.Warning("DeleteTorrentEndpoint called but not implemented. Hashes: {Hashes}", string.Join(", ", req.Hashes));
        
		HttpContext.Response.ContentType = "application/json";
		var payload = JsonSerializer.Serialize(new DeleteTorrentResponse { Success = true });
		await HttpContext.Response.WriteAsync(payload, ct);
	}
}


