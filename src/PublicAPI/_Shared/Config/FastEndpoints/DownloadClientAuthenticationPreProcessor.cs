using Microsoft.EntityFrameworkCore;
using Reaparr.Identity.Contracts;

namespace Reaparr.PublicAPI;

public class DownloadClientAuthenticationPreProcessor<TRequest> : IPreProcessor<TRequest>
{
    private readonly ILogger _log;
    private readonly IAuthDbContextFactory _authDbContextFactory;

    public DownloadClientAuthenticationPreProcessor(ILogger log, IAuthDbContextFactory authDbContextFactory)
    {
        _log = log.ForContext<DownloadClientAuthenticationPreProcessor<TRequest>>();
        _authDbContextFactory = authDbContextFactory;
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
            await ctx.HttpContext.Response.SendForbiddenAsync(cancellation: ct);
            return;
        }

        if (!(await IsValidSession(sid, ct)))
        {
            ExpireSidCookie(ctx.HttpContext);
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

            await ctx.HttpContext.Response.SendForbiddenAsync(cancellation: ct);
        }
    }

    private static void ExpireSidCookie(HttpContext httpContext)
    {
        var isHttps = httpContext.Request.IsHttps;
        var options = new CookieOptions
        {
            Path = "/",
            HttpOnly = true,
            Secure = isHttps,
            SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = DateTimeOffset.UnixEpoch,
        };
        httpContext.Response.Cookies.Delete("SID", options);
    }

    private async Task<bool> IsValidSession(string sid, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(sid))
            return false;

        using var authDbContext = await _authDbContextFactory.CreateAsync();

        var entity = await authDbContext.DownloadClientSessions.FirstOrDefaultAsync(x => x.Sid == sid, ct);
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
            authDbContext.DownloadClientSessions.Remove(entity);
            await authDbContext.SaveChangesAsync(ct);
            return false;
        }

        return true;
    }
}
