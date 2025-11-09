using FastEndpoints;
using Reaparr.Settings.Contracts;

namespace Reaparr.PublicAPI;

public class IndexerAuthenticationPreProcessor<TRequest> : IPreProcessor<TRequest>
{
    private readonly ISonarrSettings _sonarrSettings;
    private readonly IRadarrSettings _radarrSettings;
    private readonly string _indexerApiKey = "apikey";

    public IndexerAuthenticationPreProcessor(ISonarrSettings sonarrSettings, IRadarrSettings radarrSettings)
    {
        _sonarrSettings = sonarrSettings;
        _radarrSettings = radarrSettings;
    }

    public Task PreProcessAsync(IPreProcessorContext<TRequest> ctx, CancellationToken ct)
    {
        var apiKey = ctx.HttpContext.Request.Query.TryGetValue(_indexerApiKey, out var queryKey)
                ? queryKey.ToString() : null;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            ctx.ValidationFailures.Add(new($"Missing Query parameter: {_indexerApiKey}",
                $"The [{_indexerApiKey}] query param needs to be set!"));
            return ctx.HttpContext.Response.SendErrorsAsync(ctx.ValidationFailures, cancellation: ct);
        }

        if (apiKey != _sonarrSettings.ReaparrApiKey)
            return ctx.HttpContext.Response.SendUnauthorizedAsync(cancellation: ct);

        return Task.CompletedTask;
    }
}