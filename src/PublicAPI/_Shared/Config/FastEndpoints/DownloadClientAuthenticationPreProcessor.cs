using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Reaparr.Identity.Contracts;

namespace Reaparr.PublicAPI;

public class DownloadClientAuthenticationPreProcessor<TRequest> : IPreProcessor<TRequest>
{
    private readonly ILogger _log;
    private readonly IAuthDbContext _authDbContext;

    public DownloadClientAuthenticationPreProcessor(
        ILogger log,
        IAuthDbContext authDbContext)
    {
        _log = log.ForContext<DownloadClientAuthenticationPreProcessor<TRequest>>();
        _authDbContext = authDbContext;
    }

    public async Task PreProcessAsync(IPreProcessorContext<TRequest> ctx, CancellationToken ct)
    {
        var cookies = ctx.HttpContext.Request.Cookies;
        var requestPath = ctx.HttpContext.Request.Path;
        var userAgent = ctx.HttpContext.Request.Headers["User-Agent"].ToString();
        if (!cookies.TryGetValue("SID", out var sid) || string.IsNullOrWhiteSpace(sid))
        {
            _log.Here().Warning("Missing download client session SID cookie from {UserAgent} for request to '{RequestPath}'",
                userAgent, requestPath);
            await ctx.HttpContext.Response.SendUnauthorizedAsync(cancellation: ct);
            return;
        }

        if (!(await IsValidSession(sid, ct)))
        {
            _log.Here().Warning("Invalid or expired download client session SID cookie from {UserAgent} for request to '{RequestPath}'",
                userAgent, requestPath);
            
            if (userAgent.Contains("Radarr") || userAgent.Contains("Sonarr"))
            {
                _log.Information( "Downloading client appears to be Radarr or Sonarr. Sometimes an old SID cookie is cached and restarting Radarr/Sonarr can resolve this issue.");
            }
            
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