using FastEndpoints;

namespace Reaparr.PublicAPI;

public class DownloadClientAuthenticationPreProcessor<TRequest> : IPreProcessor<TRequest>
{
    private readonly IDownloadClientSessionManager _sessionManager;

    public DownloadClientAuthenticationPreProcessor(IDownloadClientSessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }

    public Task PreProcessAsync(IPreProcessorContext<TRequest> ctx, CancellationToken ct)
    {
        var cookies = ctx.HttpContext.Request.Cookies;
        if (!cookies.TryGetValue("SID", out var sid) || string.IsNullOrWhiteSpace(sid))
        {
            return ctx.HttpContext.Response.SendUnauthorizedAsync(cancellation: ct);
        }

        if (!_sessionManager.IsValidSession(sid))
        {
            return ctx.HttpContext.Response.SendUnauthorizedAsync(cancellation: ct);
        }

        return Task.CompletedTask;
    }
}