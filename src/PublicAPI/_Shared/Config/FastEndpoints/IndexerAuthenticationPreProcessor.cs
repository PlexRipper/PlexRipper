using FastEndpoints;
using Reaparr.Settings.Contracts;

namespace Reaparr.PublicAPI;

public class IndexerAuthenticationPreProcessor<TRequest> : IPreProcessor<TRequest>
{
    private readonly ILogger _log;
    private readonly IIntegrationsSettings _integrationsSettings;
    private const string INDEXER_API_KEY = "apikey";

    public IndexerAuthenticationPreProcessor(ILogger log, IIntegrationsSettings integrationsSettings)
    {
        _log = log.ForContext<IndexerAuthenticationPreProcessor<TRequest>>();
        _integrationsSettings = integrationsSettings;
    }

    public Task PreProcessAsync(IPreProcessorContext<TRequest> ctx, CancellationToken ct)
    {
        var apiKey = ctx.HttpContext.Request.Query.TryGetValue(INDEXER_API_KEY, out var queryKey)
            ? queryKey.ToString()
            : null;
        var requestPath = ctx.HttpContext.Request.Path;
        var userAgent = ctx.HttpContext.Request.Headers["User-Agent"].ToString();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _log.Here()
                .Warning(
                    "Missing indexer API key from {UserAgent} for request to '{RequestPath}'",
                    userAgent,
                    requestPath
                );
            ctx.ValidationFailures.Add(
                new(
                    $"Missing Query parameter: {INDEXER_API_KEY}",
                    $"The [{INDEXER_API_KEY}] query param needs to be set!"
                )
            );
            return ctx.HttpContext.Response.SendErrorsAsync(ctx.ValidationFailures, cancellation: ct);
        }

        if (apiKey != _integrationsSettings.ReaparrApiKey)
        {
            _log.Here()
                .Warning(
                    "Invalid indexer API key from {UserAgent} for request to '{RequestPath}'",
                    userAgent,
                    requestPath
                );
            return ctx.HttpContext.Response.SendUnauthorizedAsync(cancellation: ct);
        }

        return Task.CompletedTask;
    }
}
