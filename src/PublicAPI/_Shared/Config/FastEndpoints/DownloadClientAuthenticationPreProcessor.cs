using FastEndpoints;
using Microsoft.EntityFrameworkCore;
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

    public async Task PreProcessAsync(IPreProcessorContext<TRequest> ctx, CancellationToken ct)
    {
        var cookies = ctx.HttpContext.Request.Cookies;
        if (!cookies.TryGetValue("SID", out var sid) || string.IsNullOrWhiteSpace(sid))
        {
            await ctx.HttpContext.Response.SendUnauthorizedAsync(cancellation: ct);
            return;
        }

        if (!(await IsValidSession(sid, ct)))
        {
            await ctx.HttpContext.Response.SendUnauthorizedAsync(cancellation: ct);
        }
    }

    private async Task<bool> IsValidSession(string sid, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(sid))
            return false;

        var entity = await _authDbContext.DownloadClientSessions.FirstOrDefaultAsync(x => x.Sid == sid, ct);
        if (entity is null)
            return false;

        if (entity.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            _authDbContext.DownloadClientSessions.Remove(entity);
            await _authDbContext.SaveChangesAsync(ct);
            return false;
        }

        return true;
    }
}