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
	public override void Configure()
	{
		Post(PublicApiRoutes.DownloadClient + "/torrents/delete");
        Description(x => x.IsDownloadClient());
		AllowAnonymous();
	}

	public override async Task HandleAsync(DeleteTorrentRequest req, CancellationToken ct)
	{
		HttpContext.Response.ContentType = "application/json";
		var payload = JsonSerializer.Serialize(new DeleteTorrentResponse { Success = true });
		await HttpContext.Response.WriteAsync(payload, ct);
	}
}


