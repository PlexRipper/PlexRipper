using System.Net;
using Polly;
using Polly.Retry;

namespace Reaparr.Application;

internal sealed class DefaultHttpClientRetryHandler(ILogger log) : DelegatingHandler
{
    private const int MAX_RETRY_ATTEMPTS = 3;

    private readonly ILogger _log = log.ForContext<DefaultHttpClientRetryHandler>();

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var retryPipeline = CreateRetryPipeline(request.RequestUri?.ToString() ?? "unknown");

        return await retryPipeline.ExecuteAsync(
            async token =>
            {
                var requestClone = await CloneAsync(request, token);
                return await base.SendAsync(requestClone, token);
            },
            cancellationToken
        );
    }

    private ResiliencePipeline<HttpResponseMessage> CreateRetryPipeline(string requestUri)
    {
        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(
                new RetryStrategyOptions<HttpResponseMessage>
                {
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .Handle<TimeoutException>()
                        .HandleResult(response =>
                            response.StatusCode
                                is HttpStatusCode.RequestTimeout
                                    or HttpStatusCode.InternalServerError
                                    or HttpStatusCode.BadGateway
                                    or HttpStatusCode.ServiceUnavailable
                                    or HttpStatusCode.GatewayTimeout
                        ),
                    MaxRetryAttempts = MAX_RETRY_ATTEMPTS,
                    Delay = TimeSpan.FromMilliseconds(200),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = false,
                    OnRetry = args =>
                    {
                        args.Outcome.Result?.Dispose();

                        _log.Here()
                            .Warning(
                                "HTTP request to {RequestUri} failed, retrying attempt {RetryAttempt} of {RetryCount} in {RetryDelay}",
                                requestUri,
                                args.AttemptNumber + 1,
                                MAX_RETRY_ATTEMPTS,
                                args.RetryDelay
                            );

                        return default;
                    },
                }
            )
            .Build();
    }

    private static async Task<HttpRequestMessage> CloneAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        if (request.Content is not null)
        {
            var memoryStream = new MemoryStream();
            await request.Content.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
            memoryStream.Position = 0;
            clone.Content = new StreamContent(memoryStream);

            foreach (var header in request.Content.Headers)
                clone.Content.Headers.Add(header.Key, header.Value);
        }

        clone.Version = request.Version;
        clone.VersionPolicy = request.VersionPolicy;

        foreach (var option in request.Options)
            clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        return clone;
    }
}
