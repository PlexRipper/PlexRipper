using System.Text;
using System.Xml.Serialization;
using FastEndpoints;
using FluentValidation;

namespace Reaparr.PublicAPI;

public class TorznabEndpointRequestValidator : Validator<TorznabEndpointRequest>
{
    public TorznabEndpointRequestValidator()
    {
        RuleFor(x => x.Type).NotEmpty();
    }
}

public sealed class TorznabEndpoint : Endpoint<TorznabEndpointRequest>
{
    private readonly ICommandExecutor _commandExecutor;

    public override void Configure()
    {
        Get(PublicApiRoutes.Indexer);
        Description(x => x.IsIndexer());
        AllowAnonymous();
    }

    public TorznabEndpoint(ICommandExecutor commandExecutor)
    {
        _commandExecutor = commandExecutor;
    }

    public override async Task HandleAsync(TorznabEndpointRequest req, CancellationToken ct)
    {
        switch (req.Type)
        {
            case "caps":
                await SendCapsAsync(cancellation: ct);
                break;
            case "search":
                throw new NotImplementedException();

                break;
            case "tvsearch":
                await SendTvSearchAsync(cancellation: ct);
                break;
            case "movie":
                throw new NotImplementedException();

                break;
            default:
                throw new NotImplementedException();
                break;
        }
    }

    private async Task SendCapsAsync(CancellationToken cancellation)
    {
        var response = await _commandExecutor.Send(new GetCapabilitiesCommand(), cancellation);

        await Send.XMLAsync(response, cancellationToken: cancellation);
    }

    private async Task SendTvSearchAsync(CancellationToken cancellation)
    {
        HttpContext.Response.ContentType = "application/rss+xml";

        var xml = """
                  <rss version="2.0" xmlns:torznab="http://torznab.com/schemas/2015/feed">
                    <channel>
                      <title>Reaparr Torznab</title>
                      <link>http://localhost:5000/indexer/api</link>
                      <description>Mock search results</description>
                      <language>en-us</language>
                      <item>
                        <title>Example TV Show S01E01 1080p</title>
                        <link>http://example.com/download/12345</link>
                        <guid isPermaLink="false">example-tv-show-s01e01-1080p</guid>
                        <pubDate>Mon, 01 Jan 2024 00:00:00 +0000</pubDate>
                        <category>TV</category>
                        <category>HD</category>
                        <enclosure url="http://example.com/download/12345" length="123456789" type="application/x-bittorrent"/>
                        <torznab:attr name="size" value="123456789"/>
                        <torznab:attr name="seeders" value="100"/>
                        <torznab:attr name="leechers" value="10"/>
                        <torznab:attr name="peers" value="110"/>
                        <torznab:attr name="infohash" value="abcdef1234567890abcdef1234567890abcdef12"/>
                        <description>Example TV Show S01E01 1080p HDTV x264</description>
                      </item>
                    </channel>
                  </rss>
                  """;

        await HttpContext.Response.WriteAsync(xml, cancellation);
    }
}