namespace Reaparr.IntegrationTests;

public class RemovedPhotoBrowseEndpointsIntegrationTests : BaseIntegrationTests
{
    [Test]
    public async Task ShouldNotExposeRemovedPhotoBrowseRoutes()
    {
        // Arrange
        var seed = new Seed(29003);
        using var container = await CreateContainer(
            seed,
            config =>
            {
                config.HttpClientOptions = (x, _) => x.SetupIdentityRequest(seed);
                config.DatabaseOptions = x =>
                {
                    x.PlexAccountCount = 1;
                    x.PlexServerCount = 1;
                    x.PlexPhotoLibraryCount = 1;
                    x.PhotoAlbumCount = 1;
                    x.PhotoCount = 1;
                };
            }
        );
        using var context = await container.Resolve<IReaparrDbContextFactory>().CreateAsync();
        var library = await context.PlexLibraries.SingleAsync(x => x.Type == PlexMediaType.PhotoAlbum, CancellationToken);
        var root = await context.PlexPhotoFolders.SingleAsync(
            x => x.PlexLibraryId == library.Id && x.ParentId == null,
            CancellationToken
        );
        var album = await context.PlexPhotoAlbums.SingleAsync(x => x.PlexLibraryId == library.Id, CancellationToken);
        var asset = await context.PlexPhotoMediaData.SingleAsync(x => x.PlexLibraryId == library.Id, CancellationToken);

        using var client = container.GetApiClient();
        await client.SignIn();

        var removedGetRoutes = new[]
        {
            $"{ApiRoutes.PlexLibraryController}/{library.Id}/photo-source-roots",
            $"{ApiRoutes.PlexLibraryController}/{library.Id}/photo-folders?sourceRootId={root.Id}&page=1&pageSize=100",
            $"{ApiRoutes.PlexLibraryController}/{library.Id}/photo-albums?page=1&pageSize=100",
            $"{ApiRoutes.PlexLibraryController}/{library.Id}/photo-albums/{album.Id}/assets?page=1&pageSize=100",
            $"{ApiRoutes.PlexLibraryController}/{library.Id}/photo-assets/{asset.Id}",
        };

        // Act
        var getStatusCodes = new List<HttpStatusCode>();
        foreach (var route in removedGetRoutes)
        {
            using var response = await client.GetAsync(route, CancellationToken);
            getStatusCodes.Add(response.StatusCode);
        }

        var expansionRequest =
            $"{{\"plexLibraryId\":{library.Id},\"selections\":[{{\"kind\":\"Asset\",\"id\":{asset.Id}}}]}}";
        using var expansionContent = new StringContent(
            expansionRequest,
            System.Text.Encoding.UTF8,
            "application/json"
        );
        using var expansionResponse = await client.PostAsync(
            $"{ApiRoutes.PlexLibraryController}/{library.Id}/photo-selections/expand",
            expansionContent,
            CancellationToken
        );

        // Assert
        getStatusCodes.ShouldBe(
            [
                HttpStatusCode.NotFound,
                HttpStatusCode.NotFound,
                HttpStatusCode.NotFound,
                HttpStatusCode.NotFound,
                HttpStatusCode.NotFound,
            ]
        );
        expansionResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
