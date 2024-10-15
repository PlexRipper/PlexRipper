using System.Net;
using System.Text;
using System.Text.Json;
using Moq.Contrib.HttpClient;
using PlexApi.Contracts;
using PlexRipper.Domain.Config;
using PlexRipper.PlexApi;

namespace PlexApi.UnitTests;

public class PlexApiClientUnitTests : BaseUnitTest<Func<PlexApiClientOptions?, PlexApiClient>>
{
    public PlexApiClientUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnValid401ResponseAsJson_WhenPlexApiReturns401HtmlResponse()
    {
        SetupHttpClient(config =>
        {
            config
                .SetupAnyRequest()
                .ReturnsResponse(
                    HttpStatusCode.Unauthorized,
                    x =>
                    {
                        x.Content = new StringContent(
                            "<html><head><title>Unauthorized</title></head><body><h1>401 Unauthorized</h1></body></html>",
                            Encoding.UTF8,
                            "text/html"
                        );
                    }
                );
        });

        // Arrange
        var client = _sut(new PlexApiClientOptions { ConnectionUrl = "http://localhost", Action = null });

        // Act
        var responseMessage = await client.SendAsync(new HttpRequestMessage());

        // Assert
        responseMessage.ShouldNotBeNull();
        responseMessage
            .Content.Headers.Any(x => x.Key == "Content-Type" && x.Value.Any(y => y.Contains("application/json")))
            .ShouldBeTrue();
        var json = await responseMessage.Content.ReadAsStringAsync();
        json.ShouldNotBeNullOrEmpty();

        // Verify json response
        var result = JsonSerializer.Deserialize<PlexErrorDTO>(json, DefaultJsonSerializerOptions.ConfigStandard);
        result.ShouldNotBeNull();
        result.Code.ShouldBe(401);
        result.Message.ShouldBe("Unauthorized");
        result.Status.ShouldBe(401);
    }

    [Fact]
    public async Task ShouldHandleTimeoutException_WhenHttpRequestTimesOut()
    {
        // Set up the mocked HttpClient to throw a timeout exception
        SetupHttpClient(config =>
        {
            config.SetupAnyRequest().ThrowsAsync(new TaskCanceledException("Request timed out"));
        });

        // Arrange
        var client = _sut(new PlexApiClientOptions { ConnectionUrl = "http://localhost", Action = null });

        // Act
        var responseMessage = await client.SendAsync(new HttpRequestMessage());

        // Assert
        responseMessage.ShouldNotBeNull();
        responseMessage.StatusCode.ShouldBe(HttpStatusCode.RequestTimeout);
        responseMessage.ReasonPhrase.ShouldBe("Request Timeout");
    }

    [Fact]
    public async Task ShouldReturn500Response_WhenInternalServerErrorOccurs()
    {
        // Set up the mocked HttpClient to return a 500 Internal Server Error response
        SetupHttpClient(config =>
        {
            config
                .SetupAnyRequest()
                .ReturnsResponse(
                    HttpStatusCode.InternalServerError,
                    x =>
                    {
                        x.Content = new StringContent(
                            "<html><head><title>Internal Server Error</title></head><body><h1>500 Internal Server Error</h1></body></html>",
                            Encoding.UTF8,
                            "text/html"
                        );
                    }
                );
        });

        // Arrange
        var client = _sut(new PlexApiClientOptions { ConnectionUrl = "http://localhost", Action = null });

        // Act
        var responseMessage = await client.SendAsync(new HttpRequestMessage());

        // Assert
        responseMessage.ShouldNotBeNull();
        responseMessage.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        var json = await responseMessage.Content.ReadAsStringAsync();
        json.ShouldNotBeNullOrEmpty();

        // Verify JSON response content
        var result = JsonSerializer.Deserialize<PlexErrorDTO>(json, DefaultJsonSerializerOptions.ConfigStandard);
        result.ShouldNotBeNull();
        result.Message.ShouldBe("Internal Server Error");
    }

    [Fact]
    public async Task ShouldReturnValid200Response_WhenRequestIsSuccessful()
    {
        // Set up the mocked HttpClient to return a 200 OK response
        SetupHttpClient(config =>
        {
            config
                .SetupAnyRequest()
                .ReturnsResponse(
                    HttpStatusCode.OK,
                    x =>
                        x.Content = new StringContent("{ \"message\": \"Success\" }", Encoding.UTF8, "application/json")
                );
        });

        // Arrange
        var client = _sut(new PlexApiClientOptions { ConnectionUrl = "http://localhost", Action = null });

        // Act
        var responseMessage = await client.SendAsync(new HttpRequestMessage());

        // Assert
        responseMessage.ShouldNotBeNull();
        responseMessage.StatusCode.ShouldBe(HttpStatusCode.OK);
        var json = await responseMessage.Content.ReadAsStringAsync();
        json.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task ShouldRetryThreeTimes_When503ServiceUnavailableIsReceived()
    {
        var url = "http://localhost/";
        SetupHttpClient(config =>
        {
            config
                .SetupRequestSequence("http://localhost/")
                .ReturnsResponse(HttpStatusCode.ServiceUnavailable) // First retry
                .ReturnsResponse(HttpStatusCode.ServiceUnavailable) // Second retry
                .ReturnsResponse(HttpStatusCode.OK, "{ \"message\": \"Success\" }"); // Successful after retries
        });

        // Arrange
        var client = _sut(
            new PlexApiClientOptions
            {
                ConnectionUrl = "http://localhost",
                RetryCount = 3,
                Action = null,
            }
        );

        // Act
        var responseMessage = await client.SendAsync(new HttpRequestMessage());

        // Assert
        responseMessage.ShouldNotBeNull();
        responseMessage.StatusCode.ShouldBe(HttpStatusCode.OK);
        var json = await responseMessage.Content.ReadAsStringAsync();
        json.ShouldBe("{ \"message\": \"Success\" }");

        HttpHandlerMock.VerifyRequest(url, Times.Exactly(3));
    }

    [Fact]
    public async Task ShouldThrowException_WhenHttpRequestExceptionOccurs()
    {
        // Set up the mocked HttpClient to throw an HttpRequestException
        SetupHttpClient(config =>
        {
            config.SetupAnyRequest().ThrowsAsync(new HttpRequestException("Network error"));
        });

        // Arrange
        var client = _sut(new PlexApiClientOptions { ConnectionUrl = "http://localhost", Action = null });

        // Act
        var responseMessage = await client.SendAsync(new HttpRequestMessage());

        // Assert
        responseMessage.ShouldNotBeNull();
        responseMessage.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
