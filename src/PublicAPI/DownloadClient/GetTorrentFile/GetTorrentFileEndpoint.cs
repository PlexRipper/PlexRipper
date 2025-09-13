using System.Text;
using FastEndpoints;

namespace Reaparr.PublicAPI;

public sealed class GetTorrentFileEndpoint : EndpointWithoutRequest
{
	public override void Configure()
	{
		Get(PublicApiRoutes.DownloadClient + "/torrents/file/{hash}.torrent");
        Description(x => x.IsDownloadClient());
		AllowAnonymous();
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var bytes = Encoding.UTF8.GetBytes("d8:announce0:e");
		HttpContext.Response.ContentType = "application/x-bittorrent";
		await HttpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length, ct);
	}
}


