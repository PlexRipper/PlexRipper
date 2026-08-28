namespace Reaparr.PublicAPI;

/// <summary>
/// Authenticates the torrent file endpoint with EITHER a download client session (SID cookie)
/// OR the indexer API key.
///
/// The Torznab feed hands Sonarr/Radarr a link to this endpoint, but the *arrs fetch release
/// URLs through their indexer HTTP path, which authenticates with the indexer API key and
/// carries no download client cookie jar. Requiring a SID there means the grab can never
/// succeed, so accept the indexer API key for this one endpoint as well.
/// </summary>
public class DownloadTorrentAuthenticationPreProcessor<TRequest> : IPreProcessor<TRequest>
{
    private const string INDEXER_API_KEY = "apikey";

    private readonly ILogger _log;
    private readonly IAuthDbContextFactory _authDbContextFactory;
    private readonly IIntegrationsSettings _integrationsSettings;

    public DownloadTorrentAuthenticationPreProcessor(
        ILogger log,
        IAuthDbContextFactory authDbContextFactory,
        IIntegrationsSettings integrationsSettings
    )
    {
        _log = log.ForContext<DownloadTorrentAuthenticationPreProcessor<TRequest>>();
        _authDbContextFactory = authDbContextFactory;
        _integrationsSettings = integrationsSettings;
    }

    public async Task PreProcessAsync(IPreProcessorContext<TRequest> ctx, CancellationToken ct)
    {
        var requestPath = ctx.HttpContext.Request.Path;
        var userAgent = ctx.HttpContext.Request.Headers["User-Agent"].ToString();

        // 1. The indexer API key, which is what Sonarr/Radarr send when fetching a release URL.
        if (ctx.HttpContext.Request.Query.TryGetValue(INDEXER_API_KEY, out var queryKey))
        {
            var apiKey = queryKey.ToString();
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                if (apiKey == _integrationsSettings.ReaparrApiKey)
                    return;

                _log.Here()
                    .Warning(
                        "Invalid indexer API key from {UserAgent} for request to '{RequestPath}'",
                        userAgent,
                        requestPath
                    );
                await ctx.HttpContext.Response.SendUnauthorizedAsync(cancellation: ct);
                return;
            }
        }

        // 2. Fall back to a download client session, for callers that do hold one.
        ctx.HttpContext.Request.Cookies.TryGetValue(DownloadClientSessionAuthenticator.SID_COOKIE, out var sid);

        if (string.IsNullOrWhiteSpace(sid))
        {
            _log.Here()
                .Warning(
                    "Missing both indexer API key and download client session SID cookie from {UserAgent} for request to '{RequestPath}'",
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
            await ctx.HttpContext.Response.SendForbiddenAsync(cancellation: ct);
        }
    }
}
