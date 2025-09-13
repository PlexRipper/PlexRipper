using FastEndpoints;

namespace Reaparr.PublicAPI;

public record AddTorrentRequest
{
	public string Urls { get; set; } = string.Empty; // magnet or .torrent URL per qbittorrent API
}

public record AddTorrentResponse
{
	public bool Success { get; set; }
	public string Hash { get; set; } = string.Empty;
}

public sealed class AddTorrentEndpoint : Endpoint<AddTorrentRequest, AddTorrentResponse>
{
	public override void Configure()
	{
		Post(PublicApiRoutes.DownloadClient + "/torrents/add");
        Description(x => x.IsDownloadClient());

		AllowAnonymous();
	}

	public override async Task HandleAsync(AddTorrentRequest req, CancellationToken ct)
	{
		var hash = "ABC123DEF4567890ABC123DEF4567890ABC12345";
        
		await Send.OkAsync(new AddTorrentResponse { Success = true, Hash = hash }, cancellation: ct);
	}
}


