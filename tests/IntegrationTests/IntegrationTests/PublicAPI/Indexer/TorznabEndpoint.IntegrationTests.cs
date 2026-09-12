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
}
