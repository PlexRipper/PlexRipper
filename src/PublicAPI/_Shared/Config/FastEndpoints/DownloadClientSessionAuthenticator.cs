namespace Reaparr.PublicAPI;

/// <summary>
/// Shared download client session (SID cookie) validation, used by the public API
/// authentication pre-processors so the session handling only lives in one place.
/// </summary>
public static class DownloadClientSessionAuthenticator
{
    public const string SID_COOKIE = "SID";

    /// <summary>
    /// Validates the SID cookie against the stored download client sessions, removing the
    /// session when it has expired.
    /// </summary>
    public static async Task<bool> IsValidSessionAsync(
        IAuthDbContextFactory authDbContextFactory,
        ILogger log,
        string? sid,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(sid))
            return false;

        using var authDbContext = await authDbContextFactory.CreateAsync();

        var entity = await authDbContext.DownloadClientSessions.FirstOrDefaultAsync(x => x.Sid == sid, ct);
        if (entity is null)
            return false;

        if (entity.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            log.Here()
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

    public static void ExpireSidCookie(HttpContext httpContext)
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
        httpContext.Response.Cookies.Delete(SID_COOKIE, options);
    }
}
