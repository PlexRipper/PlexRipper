using System.Xml.Linq;

namespace Reaparr.IntegrationTests;

public class TorznabEndpointIntegrationTests : BaseIntegrationTests
{
    [Test]
    public async Task ShouldReturnPagedStableRssXml_WhenRadarrPollsQuerylessSearch()
    {
        // Arrange
        using var container = await CreateContainer(
            new Seed(8663),
            config =>
            {
                config.DatabaseOptions = x =>
                {
                    x.PlexServerCount = 1;
                    x.PlexAccountCount = 1;
                    x.PlexMovieLibraryCount = 1;
                    x.MovieCount = 4;
                    x.IncludeMultiPartMovies = true;
                    x.RadarrIntegrationCount = 1;
                };
            }
        );
        var integration = await container.DbContext.RadarrIntegrations.SingleAsync(CancellationToken);
        var route = $"/api/public/integrations/{integration.Id}/indexer/api";
        var url = $"{route}?t=search&cat=2000&limit=3&offset=1&apikey={integration.TorznabApiKey}";
        var client = container.GetApiClient();

        // Act
        var firstResponse = await client.GetAsync(url, CancellationToken);
        var firstXml = await firstResponse.Content.ReadAsStringAsync(CancellationToken);
        var secondResponse = await client.GetAsync(url, CancellationToken);
        var secondXml = await secondResponse.Content.ReadAsStringAsync(CancellationToken);

        // Assert
        firstResponse.IsSuccessStatusCode.ShouldBeTrue();
        secondResponse.IsSuccessStatusCode.ShouldBeTrue();
        firstResponse.Content.Headers.ContentType!.MediaType.ShouldBe("application/xml");
        firstXml.ShouldBe(secondXml);

        var document = XDocument.Parse(firstXml);
        var root = document.Root;
        root.ShouldNotBeNull();
        root.Name.LocalName.ShouldBe("rss");
        root.Attribute("version")!.Value.ShouldBe("2.0");
        root.GetNamespaceOfPrefix("torznab")!.NamespaceName.ShouldBe("http://torznab.com/schemas/2015/feed");
        root.GetNamespaceOfPrefix("newznab")!.NamespaceName.ShouldBe("http://www.newznab.com/DTD/2010/feeds/attributes/");

        var channel = root.Element("channel");
        channel.ShouldNotBeNull();
        channel.Element("title")!.Value.ShouldBe("Reaparr Indexer");
        channel.Element("description")!.Value.ShouldBe("Reaparr RSS feed");
        channel.Element("language")!.Value.ShouldBe("en-us");
        var response = channel.Elements().Single(x => x.Name.LocalName == "response");
        response.Attribute("offset")!.Value.ShouldBe("1");
        response.Attribute("total")!.Value.ShouldBe("8");

        var items = channel.Elements("item").ToList();
        items.Count.ShouldBe(3);
        var publicationDates = items.Select(x => DateTimeOffset.ParseExact(x.Element("pubDate")!.Value, "R", null)).ToList();
        publicationDates.ShouldBe(publicationDates.OrderByDescending(x => x));
        foreach (var item in items)
        {
            var guid = item.Element("guid");
            var link = item.Element("link")!.Value;
            var size = long.Parse(item.Element("size")!.Value);
            var enclosure = item.Element("enclosure");
            var attributes = item.Elements().Where(x => x.Name.LocalName == "attr").ToList();

            guid.ShouldNotBeNull();
            guid.Attribute("isPermaLink")!.Value.ShouldBe("false");
            guid.Value.ShouldStartWith("reaparr-");
            guid.Value.ShouldNotBe(link);
            attributes.Single(x => x.Attribute("name")!.Value == "size").Attribute("value")!.Value.ShouldBe(size.ToString());
            attributes.Single(x => x.Attribute("name")!.Value == "category").Attribute("value")!.Value.ShouldNotBeEmpty();
            enclosure.ShouldNotBeNull();
            enclosure.Attribute("url")!.Value.ShouldBe(link);
            enclosure.Attribute("length")!.Value.ShouldBe(size.ToString());
            enclosure.Attribute("type")!.Value.ShouldBe("application/x-bittorrent");
            link.ShouldContain("apikey=");
        }
    }

    [Test]
    public async Task ShouldReturnTorznabErrorXml_WhenOffsetIsNegative() =>
        await AssertInvalidRequest("offset=-1", 8664);

    [Test]
    public async Task ShouldReturnTorznabErrorXml_WhenLimitIsNegative() =>
        await AssertInvalidRequest("limit=-1", 8665);

    [Test]
    public async Task ShouldReturnTorznabErrorXml_WhenTypeIsUnknown() =>
        await AssertInvalidRequest("t=book", 8666);

    [Test]
    public async Task ShouldReturnTorznabErrorXml_WhenExtendedValueIsInvalid() =>
        await AssertInvalidRequest("extended=2", 8667);

    [Test]
    public async Task ShouldReturnTorznabErrorXml_WhenAttributesAreMalformed() =>
        await AssertInvalidRequest("attrs=size%2Ccategory%21", 8668);

    private async Task AssertInvalidRequest(string invalidQuery, int seedValue)
    {
        // Arrange
        using var container = await CreateContainer(
            new Seed(seedValue),
            config => config.DatabaseOptions = x => x.RadarrIntegrationCount = 1
        );
        var integration = await container.DbContext.RadarrIntegrations.SingleAsync(CancellationToken);
        var route = $"/api/public/integrations/{integration.Id}/indexer/api";
        var separator = invalidQuery.StartsWith("t=", StringComparison.Ordinal) ? string.Empty : "t=search&";
        var url = $"{route}?{separator}{invalidQuery}&apikey={integration.TorznabApiKey}";
        var client = container.GetApiClient();

        // Act
        var response = await client.GetAsync(url, CancellationToken);
        var xml = await response.Content.ReadAsStringAsync(CancellationToken);

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/xml");
        var error = XDocument.Parse(xml).Root;
        error.ShouldNotBeNull();
        error.Name.LocalName.ShouldBe("error");
        error.Attribute("code")!.Value.ShouldBe("201");
        error.Attribute("description")!.Value.ShouldBe("Incorrect parameter");
    }
}
