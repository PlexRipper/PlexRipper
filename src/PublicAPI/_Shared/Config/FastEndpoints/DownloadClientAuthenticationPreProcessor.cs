using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Reaparr.Identity.Contracts;

namespace Reaparr.PublicAPI;

public class DownloadClientAuthenticationPreProcessor<TRequest> : IPreProcessor<TRequest>
{
    private readonly ILogger _log;
    private readonly IAuthDbContext _authDbContext;

    public DownloadClientAuthenticationPreProcessor(ILogger log, IAuthDbContext authDbContext)
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
            _log.Here()
                .Warning(
                    "Missing download client session SID cookie from {UserAgent} for request to '{RequestPath}'",
                    userAgent,
                    requestPath
                );
            await ctx.HttpContext.Response.SendUnauthorizedAsync(cancellation: ct);
            return;
        }

        if (!(await IsValidSession(sid, ct)))
        {
            _log.Here()
                .Warning(
                    "Invalid or expired download client session SID cookie from {UserAgent} for request to '{RequestPath}'",
                    userAgent,
                    requestPath
                );

            var detectedClient =
                userAgent.IndexOf("radarr", StringComparison.OrdinalIgnoreCase) >= 0
                    ? "Radarr"
                    : (userAgent.IndexOf("sonarr", StringComparison.OrdinalIgnoreCase) >= 0 ? "Sonarr" : null);
            if (detectedClient is not null)
            {
                _log.Here()
                    .Warning(
                        "Download client appears to be {Client}. Sometimes an old SID cookie is cached; restarting the client can resolve this. User-Agent: {UserAgent}",
                        detectedClient,
                        userAgent
                    );
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
            _log.Here()
                .Debug(
                    "Download client session with SID '{Sid}' has expired at {ExpiresAt} and will be removed.",
                    sid,
                    entity.ExpiresAt
                );
            _authDbContext.DownloadClientSessions.Remove(entity);
            await _authDbContext.SaveChangesAsync(ct);
            return false;
        }

        return true;
    }
}
