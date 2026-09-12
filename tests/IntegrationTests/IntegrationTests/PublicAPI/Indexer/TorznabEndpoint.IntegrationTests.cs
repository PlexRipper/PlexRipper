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
        firstXml.ShouldBe(secondXml);
        firstXml.ShouldContain("xmlns:torznab=\"http://torznab.com/schemas/2015/feed\"");
        firstXml.ShouldContain("xmlns:newznab=\"http://www.newznab.com/DTD/2010/feeds/attributes/\"");
        firstXml.ShouldContain("newznab:response offset=\"1\"");
        firstXml.ShouldContain("torznab:attr name=\"category\"");
        firstXml.ShouldContain("<guid isPermaLink=\"false\">");
        firstXml.ShouldContain("<enclosure url=");
        firstXml.ShouldContain("apikey=");
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
