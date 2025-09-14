using FastEndpoints;

namespace Reaparr.PublicAPI;

public sealed class TorrentInfoResponse
{
	public string Hash { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public double Progress { get; set; }
	public string State { get; set; } = string.Empty;
	public int Eta { get; set; }
	public long Size { get; set; }
	public long Downloaded { get; set; }
	public long Uploaded { get; set; }
}

public sealed class GetTorrentsEndpoint : EndpointWithoutRequest<List<TorrentInfoResponse>>
{
	public override void Configure()
	{
		Get(PublicApiRoutes.DownloadClient + "/torrents/info");
        Description(x => x.IsDownloadClient());
		AllowAnonymous();
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var list = new List<TorrentInfoResponse>
		{
			new TorrentInfoResponse
			{
				Hash = "ABC123DEF4567890ABC123DEF4567890ABC12345",
				Name = "Example.Movie.2020.1080p.WEB-DL.x264",
				Progress = 0.65,
				State = "downloading",
				Eta = 3600,
				Size = 2147483648,
				Downloaded = 1395864371,
				Uploaded = 123456789,
			},
			new TorrentInfoResponse
			{
				Hash = "DEF456ABC1237890DEF456ABC1237890DEF456AB",
				Name = "Example.Show.S01E01.1080p.WEB-DL.x264",
				Progress = 1.0,
				State = "stalledUP",
				Eta = 0,
				Size = 1073741824,
				Downloaded = 1073741824,
				Uploaded = 987654321,
			},
		};

		await Send.OkAsync(list, cancellation: ct);
	}
}


