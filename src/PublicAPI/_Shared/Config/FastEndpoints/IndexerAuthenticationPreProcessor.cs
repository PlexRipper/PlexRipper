using FastEndpoints;
using Reaparr.Settings.Contracts;

namespace Reaparr.PublicAPI;

public class IndexerAuthenticationPreProcessor<TRequest> : IPreProcessor<TRequest>
{
    private readonly IIntegrationsSettings _integrationsSettings;
    private const string INDEXER_API_KEY = "apikey";

    public IndexerAuthenticationPreProcessor(IIntegrationsSettings integrationsSettings)
    {
        _integrationsSettings = integrationsSettings;
    }

    public Task PreProcessAsync(IPreProcessorContext<TRequest> ctx, CancellationToken ct)
    {
        var apiKey = ctx.HttpContext.Request.Query.TryGetValue(INDEXER_API_KEY, out var queryKey)
            ? queryKey.ToString()
            : null;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            ctx.ValidationFailures.Add(new($"Missing Query parameter: {INDEXER_API_KEY}",
                $"The [{INDEXER_API_KEY}] query param needs to be set!"));
            return ctx.HttpContext.Response.SendErrorsAsync(ctx.ValidationFailures, cancellation: ct);
        }

        if (apiKey != _integrationsSettings.ReaparrApiKey)
            return ctx.HttpContext.Response.SendUnauthorizedAsync(cancellation: ct);

        return Task.CompletedTask;
    }
}