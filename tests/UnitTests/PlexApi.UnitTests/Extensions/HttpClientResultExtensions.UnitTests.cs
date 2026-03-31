using System.Net;
using Moq.Contrib.HttpClient;
using Moq.Protected;
using Reaparr.FluentResultExtensions;

namespace Reaparr.PlexApi.UnitTests;

public class HttpClientResultExtensionsUnitTests : BaseUnitTest<object>
{
    [Test]
    public async Task ShouldReturnSuccessResult_WhenResponseIsReturned()
    {
        // Arrange
        HttpHandlerMock.SetupAnyRequest().ReturnsResponse(HttpStatusCode.OK);

        using var httpClient = new HttpClient(HttpHandlerMock.Object);
        using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");

        // Act
        var result = await httpClient.SendResultAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            CancellationToken
        );

        // Verify
        HttpHandlerMock
            .Protected()
            .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task ShouldReturn408RequestTimeoutError_WhenRequestTimesOut()
    {
        // Arrange
        HttpHandlerMock.SetupAnyRequest().ThrowsAsync(new TaskCanceledException("Request timed out"));

        using var httpClient = new HttpClient(HttpHandlerMock.Object);
        using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");

        // Act
        var result = await httpClient.SendResultAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            CancellationToken
        );

        // Verify
        HttpHandlerMock
            .Protected()
            .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Has408RequestTimeout().ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturn502BadGatewayError_WhenHttpRequestFails()
    {
        // Arrange
        HttpHandlerMock.SetupAnyRequest().ThrowsAsync(new HttpRequestException("Network error"));

        using var httpClient = new HttpClient(HttpHandlerMock.Object);
        using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");

        // Act
        var result = await httpClient.SendResultAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            CancellationToken
        );

        // Verify
        HttpHandlerMock
            .Protected()
            .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Has502BadGatewayError().ShouldBeTrue();
    }
}
