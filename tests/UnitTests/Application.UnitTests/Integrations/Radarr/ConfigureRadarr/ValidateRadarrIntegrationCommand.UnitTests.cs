using System.Collections.Concurrent;
using System.Net;

namespace Reaparr.Application.UnitTests;

public class ValidateRadarrIntegrationCommandUnitTests : BaseCommandUnitTest<ValidateRadarrIntegrationCommand>
{
    [Test]
    public async Task ShouldValidateOnlySubmittedReaparrResourcesConcurrently()
    {
        // Arrange
        var integrationId = Guid.Parse("00000000-0000-0000-0000-000000055342");
        var downloadClient = new RadarrDownloadContractDTO
        {
            Fields = [new RadarrDownloadContractCreateFieldDTO { Name = "apiKey", Value = "qbt-key" }],
        };
        var indexer = new RadarrIndexerContractDTO
        {
            Fields = [new RadarrIndexerContractFieldDTO { Name = "apiKey", Value = "torznab-key" }],
        };
        var handler = new RecordingHandler();
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://radarr.test") };

        Mock.Mock<IRadarrHttpClientFactory>()
            .Setup(x => x.CreateAsync(integrationId))
            .ReturnsAsync(Result.Ok(client))
            .Verifiable(Times.Once());

        // Act
        var result = await TestHandlerExecuteAsync(
            new ValidateRadarrIntegrationCommand(integrationId, downloadClient, indexer)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        handler.Requests.Count.ShouldBe(2);
        handler.Requests.ShouldContain(x => x.Path == "/api/v3/downloadclient/test" && x.Body.Contains("qbt-key"));
        handler.Requests.ShouldContain(x => x.Path == "/api/v3/indexer/test" && x.Body.Contains("torznab-key"));
        handler.MaximumConcurrency.ShouldBe(2);
        Mock.Mock<IRadarrHttpClientFactory>().Verify();
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private int _activeRequests;
        private int _maximumConcurrency;

        public ConcurrentBag<(string Path, string Body)> Requests { get; } = [];
        public int MaximumConcurrency => _maximumConcurrency;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            var activeRequests = Interlocked.Increment(ref _activeRequests);
            Interlocked.Exchange(ref _maximumConcurrency, Math.Max(_maximumConcurrency, activeRequests));
            Requests.Add((request.RequestUri!.AbsolutePath, await request.Content!.ReadAsStringAsync(cancellationToken)));
            await Task.Delay(10, cancellationToken);
            Interlocked.Decrement(ref _activeRequests);
            return new HttpResponseMessage(HttpStatusCode.OK) { RequestMessage = request };
        }
    }
}
