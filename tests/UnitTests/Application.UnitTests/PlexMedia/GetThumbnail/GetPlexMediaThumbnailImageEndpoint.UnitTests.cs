using System.Net;
using Microsoft.Extensions.Caching.Memory;

namespace Reaparr.Application.UnitTests;

public class GetPlexMediaThumbnailImageEndpointUnitTests : BaseUnitTest<GetPlexMediaThumbnailImageEndpoint>
{
    [Test]
    public void ShouldBuildTranscodeUrl_WithEncodedOriginalThumbnailUrl()
    {
        const string token = "token+with&reserved=characters";

        var url = GetPlexMediaThumbnailImageEndpoint.BuildTranscodeUrl(
            "https://plex.example:32400/",
            "/library/metadata/481523/thumb/1772632750",
            token,
            300,
            450
        );

        var uri = new Uri(url);
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

        query["width"].ToString().ShouldBe("300");
        query["height"].ToString().ShouldBe("450");
        query["url"].ToString().ShouldBe(
            "/library/metadata/481523/thumb/1772632750?X-Plex-Token=token%2Bwith%26reserved%3Dcharacters"
        );
        query["X-Plex-Token"].ToString().ShouldBe(token);
    }

    [Test]
    public void ShouldBuildDirectThumbnailUrl_WithTokenAndWithoutDoubleSlash()
    {
        var url = GetPlexMediaThumbnailImageEndpoint.BuildDirectThumbnailUrl(
            "https://plex.example:32400/",
            "/library/metadata/481523/thumb/1772632750",
            "plex-token"
        );

        url.ShouldBe(
            "https://plex.example:32400/library/metadata/481523/thumb/1772632750?X-Plex-Token=plex-token"
        );
    }

    [Test]
    public async Task ShouldReturnOriginalThumbnail_WhenPlexTranscodeFails()
    {
        // Arrange
        var handler = new StubHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.InternalServerError),
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent([1, 2, 3]),
            }
        );
        using var client = new HttpClient(handler);
        var endpoint = new GetPlexMediaThumbnailImageEndpoint(
            Mock.Create<ILogger>(),
            Mock.Create<IAppRuntimeInfo>(),
            Mock.Create<IReaparrDbContext>(),
            new Mock<IHttpClientFactory>().Object,
            new MemoryCache(new MemoryCacheOptions())
        );

        // Act
        using var response = await endpoint.GetThumbnailResponseAsync(
            client,
            "https://plex.example/photo/:/transcode",
            "https://plex.example/library/metadata/1/thumb/2",
            CancellationToken
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        handler.RequestUris.ShouldBe(
            [
                new Uri("https://plex.example/photo/:/transcode"),
                new Uri("https://plex.example/library/metadata/1/thumb/2"),
            ]
        );
    }

    [Test]
    public async Task ShouldNotRequestOriginalThumbnail_WhenPlexRejectsAccessToken()
    {
        // Arrange
        var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.Unauthorized));
        using var client = new HttpClient(handler);
        var endpoint = new GetPlexMediaThumbnailImageEndpoint(
            Mock.Create<ILogger>(),
            Mock.Create<IAppRuntimeInfo>(),
            Mock.Create<IReaparrDbContext>(),
            new Mock<IHttpClientFactory>().Object,
            new MemoryCache(new MemoryCacheOptions())
        );

        // Act
        using var response = await endpoint.GetThumbnailResponseAsync(
            client,
            "https://plex.example/photo/:/transcode",
            "https://plex.example/library/metadata/1/thumb/2",
            CancellationToken
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        handler.RequestUris.ShouldBe([new Uri("https://plex.example/photo/:/transcode")]);
    }

    [Test]
    public async Task ShouldNotRequestOriginalThumbnail_WhenPlexTranscodeSucceeds()
    {
        // Arrange
        var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
        using var client = new HttpClient(handler);
        var endpoint = new GetPlexMediaThumbnailImageEndpoint(
            Mock.Create<ILogger>(),
            Mock.Create<IAppRuntimeInfo>(),
            Mock.Create<IReaparrDbContext>(),
            new Mock<IHttpClientFactory>().Object,
            new MemoryCache(new MemoryCacheOptions())
        );

        // Act
        using var response = await endpoint.GetThumbnailResponseAsync(
            client,
            "https://plex.example/photo/:/transcode",
            "https://plex.example/library/metadata/1/thumb/2",
            CancellationToken
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        handler.RequestUris.ShouldBe([new Uri("https://plex.example/photo/:/transcode")]);
    }

    private sealed class StubHttpMessageHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new(responses);

        public List<Uri> RequestUris { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            RequestUris.Add(request.RequestUri!);
            return Task.FromResult(_responses.Dequeue());
        }
    }
}
