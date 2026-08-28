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
        if (
            !cookies.TryGetValue(DownloadClientSessionAuthenticator.SID_COOKIE, out var sid)
            || string.IsNullOrWhiteSpace(sid)
        )
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

        if (!await DownloadClientSessionAuthenticator.IsValidSessionAsync(_authDbContextFactory, _log, sid, ct))
        {
            DownloadClientSessionAuthenticator.ExpireSidCookie(ctx.HttpContext);
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
}
