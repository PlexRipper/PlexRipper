using System.Globalization;
using System.Text;
using System.Xml.Serialization;
using FastEndpoints;

namespace Reaparr.PublicAPI.Search;

public sealed class SearchEndpoint : EndpointWithoutRequest<string>
{
	public override void Configure()
	{
		Get(PublicApiRoutes.Indexer + "/search");
        Description(x => x.IsIndexer());
		AllowAnonymous();
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var serializer = new XmlSerializer(typeof(Rss));
		var rss = Rss.CreateMock(HttpContext);
		var namespaces = new XmlSerializerNamespaces();
		namespaces.Add("torznab", "http://torznab.com/schemas/2015/feed");

		var sb = new StringBuilder();
		using (var writer = new StringWriter(sb, CultureInfo.InvariantCulture))
		{
			serializer.Serialize(writer, rss, namespaces);
		}

		HttpContext.Response.ContentType = "application/rss+xml";
		await Send.OkAsync(sb.ToString(), cancellation: ct);
	}
}

[XmlRoot("rss")]
public class Rss
{
	[XmlAttribute("version")]
	public string Version { get; set; } = "2.0";

	[XmlElement("channel")]
	public RssChannel Channel { get; set; } = new();

	public static Rss CreateMock(HttpContext ctx)
	{
		var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
		return new Rss
		{
			Channel = new RssChannel
			{
				Title = "Reaparr Mock Indexer",
				Description = "Mock search results",
				Link = $"{baseUrl}/indexer/api",
				Language = "en-us",
				Items =
				[
					new RssItem
					{
						Title = "Example.Movie.2020.1080p.WEB-DL.x264",
						Guid = new RssGuid { IsPermaLink = false, Value = "reaparr-movie-abc123" },
						PubDate = DateTimeOffset.UtcNow.ToString("r", CultureInfo.InvariantCulture),
						Link = "magnet:?xt=urn:btih:ABC123&dn=Example.Movie.2020.1080p.WEB-DL.x264",
						Enclosure = new RssEnclosure
						{
							Url = $"{baseUrl}/downloadclient/api/v2/torrents/file/ABC123.torrent",
							Length = 2147483648,
							Type = "application/x-bittorrent",
						},
						TorznabAttributes =
						[
							new TorznabAttr { Name = "category", Value = "5000" },
							new TorznabAttr { Name = "size", Value = "2147483648" },
							new TorznabAttr { Name = "imdbid", Value = "tt1234567" },
							new TorznabAttr { Name = "seeders", Value = "123" },
							new TorznabAttr { Name = "peers", Value = "150" },
							new TorznabAttr { Name = "downloadvolumefactor", Value = "1" },
							new TorznabAttr { Name = "uploadvolumefactor", Value = "1" },
						],
					},
					new RssItem
					{
						Title = "Example.Show.S01E01.1080p.WEB-DL.x264",
						Guid = new RssGuid { IsPermaLink = false, Value = "reaparr-tv-def456" },
						PubDate = DateTimeOffset.UtcNow.ToString("r", CultureInfo.InvariantCulture),
						Link = "magnet:?xt=urn:btih:DEF456&dn=Example.Show.S01E01.1080p.WEB-DL.x264",
						Enclosure = new RssEnclosure
						{
							Url = $"{baseUrl}/downloadclient/api/v2/torrents/file/DEF456.torrent",
							Length = 1073741824,
							Type = "application/x-bittorrent",
						},
						TorznabAttributes =
						[
							new TorznabAttr { Name = "category", Value = "2000" },
							new TorznabAttr { Name = "size", Value = "1073741824" },
							new TorznabAttr { Name = "tvdbid", Value = "12345" },
							new TorznabAttr { Name = "season", Value = "1" },
							new TorznabAttr { Name = "episode", Value = "1" },
							new TorznabAttr { Name = "seeders", Value = "88" },
							new TorznabAttr { Name = "peers", Value = "100" },
							new TorznabAttr { Name = "downloadvolumefactor", Value = "1" },
							new TorznabAttr { Name = "uploadvolumefactor", Value = "1" },
						],
					},
				],
			},
		};
	}
}

public class RssChannel
{
	[XmlElement("title")]
	public string Title { get; set; } = string.Empty;

	[XmlElement("description")]
	public string Description { get; set; } = string.Empty;

	[XmlElement("link")]
	public string Link { get; set; } = string.Empty;

	[XmlElement("language")]
	public string Language { get; set; } = "en-us";

	[XmlElement("item")]
	public List<RssItem> Items { get; set; } = new();
}

public class RssItem
{
	[XmlElement("title")]
	public string Title { get; set; } = string.Empty;

	[XmlElement("guid")]
	public RssGuid Guid { get; set; } = new();

	[XmlElement("pubDate")]
	public string PubDate { get; set; } = string.Empty;

	[XmlElement("link")]
	public string Link { get; set; } = string.Empty;

	[XmlElement("enclosure")]
	public RssEnclosure Enclosure { get; set; } = new();

	[XmlElement("attr", Namespace = "http://torznab.com/schemas/2015/feed")]
	public List<TorznabAttr> TorznabAttributes { get; set; } = new();
}

public class RssGuid
{
	[XmlAttribute("isPermaLink")]
	public bool IsPermaLink { get; set; }

	[XmlText]
	public string Value { get; set; } = string.Empty;
}

public class RssEnclosure
{
	[XmlAttribute("url")]
	public string Url { get; set; } = string.Empty;

	[XmlAttribute("length")]
	public long Length { get; set; }

	[XmlAttribute("type")]
	public string Type { get; set; } = string.Empty;
}

public class TorznabAttr
{
	[XmlAttribute("name")]
	public string Name { get; set; } = string.Empty;

	[XmlAttribute("value")]
	public string Value { get; set; } = string.Empty;
}


