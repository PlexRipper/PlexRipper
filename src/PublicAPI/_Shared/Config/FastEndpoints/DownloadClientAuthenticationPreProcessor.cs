using FastEndpoints;
using Reaparr.Identity.Contracts;

namespace Reaparr.PublicAPI;

public class DownloadClientAuthenticationPreProcessor<TRequest> : IPreProcessor<TRequest>
{
    private readonly IAuthDbContext _authDbContext;

    public DownloadClientAuthenticationPreProcessor(
        IAuthDbContext authDbContext)
    {
        _authDbContext = authDbContext;
    }

    public Task PreProcessAsync(IPreProcessorContext<TRequest> ctx, CancellationToken ct)
    {
        var cookies = ctx.HttpContext.Request.Cookies;
        if (!cookies.TryGetValue("SID", out var sid) || string.IsNullOrWhiteSpace(sid))
        {
            return ctx.HttpContext.Response.SendUnauthorizedAsync(cancellation: ct);
        }

        if (!IsValidSession(sid))
        {
            return ctx.HttpContext.Response.SendUnauthorizedAsync(cancellation: ct);
        }

        return Task.CompletedTask;
    }

    public bool IsValidSession(string sid)
    {
        if (string.IsNullOrWhiteSpace(sid))
            return false;

        var entity = _authDbContext.DownloadClientSessions.Find(sid);
        if (entity is null)
            return false;

        if (entity.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            _authDbContext.DownloadClientSessions.Remove(entity);
            _authDbContext.SaveChangesAsync();
            return false;
        }
        
        return true;
    }
}