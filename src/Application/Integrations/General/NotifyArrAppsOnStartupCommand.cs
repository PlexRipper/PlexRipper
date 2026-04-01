using Microsoft.EntityFrameworkCore;
using Reaparr.FluentResultExtensions;

namespace Reaparr.Application;

/// <summary>
/// Fired once after Reaparr is fully listening (via <c>IHostApplicationLifetime.ApplicationStarted</c>).
/// It performs two things that together prevent Radarr/Sonarr from losing download-progress tracking
/// after Reaparr restarts.
/// </summary>
/// <remarks>
/// <b>Root cause of the problem</b><br/>
/// When Reaparr restarts there is a brief window where Radarr/Sonarr try to reach it and fail.
/// Radarr (and Sonarr) use an escalating back-off system
/// (<c>ProviderStatusServiceBase / DownloadClientStatusService</c>) that records each failure and sets
/// <c>DisabledTill</c> using the period table <c>{ 0, 60, 300, 900, 1800, 3600, … }</c> seconds.
/// Once a client's <c>DisabledTill</c> is in the future,
/// <c>DownloadClientFactory.DownloadHandlingEnabled(filterBlockedClients: true)</c> excludes it from
/// <c>DownloadMonitoringService.Refresh()</c>. Because the blocked client is never polled,
/// <c>RecordSuccess</c> is never called, <c>DisabledTill</c> never clears — a self-reinforcing deadlock
/// that only breaks when the block expires naturally (up to an hour) or Radarr/Sonarr is restarted.
///
/// <b>Fix 1 — clear stale sessions</b><br/>
/// Reaparr stores qBittorrent-compatible session SIDs in <c>DownloadClientSessions</c>. Radarr/Sonarr
/// cache the SID from the login response and reuse it on subsequent requests. After a restart the old
/// SID is gone, so requests arrive with a stale SID and are rejected with 403. Clearing the table on
/// startup forces a clean re-authentication on the very first request rather than relying on the
/// session TTL (30 min) to expire naturally.
///
/// <b>Fix 2 — call <c>POST /api/v3/downloadclient/testall</c></b><br/>
/// This endpoint on Radarr/Sonarr uses <c>_providerFactory.All()</c> — intentionally bypassing
/// <c>DownloadHandlingEnabled(filterBlockedClients: true)</c> — and calls
/// <c>_providerFactory.Test(definition)</c> for every enabled client. A passing test immediately
/// calls <c>RecordSuccess</c>, which sets <c>DisabledTill = null</c> and restores normal monitoring.
/// A 400 response is expected when one or more clients fail their test, but <c>RecordSuccess</c> is
/// still called for the clients that passed, so the response is treated as a non-fatal outcome.
/// </remarks>
public record NotifyArrAppsOnStartupCommand : ICommand<Result>;

public class NotifyArrAppsOnStartupCommandHandler : ICommandHandler<NotifyArrAppsOnStartupCommand, Result>
{
    private readonly IAuthDbContext _authDbContext;
    private readonly IRadarrSettings _radarrSettings;
    private readonly ISonarrSettings _sonarrSettings;
    private readonly IHttpClientFactory _httpClientFactory;

    public NotifyArrAppsOnStartupCommandHandler(
        IAuthDbContext authDbContext,
        IRadarrSettings radarrSettings,
        ISonarrSettings sonarrSettings,
        IHttpClientFactory httpClientFactory
    )
    {
        _authDbContext = authDbContext;
        _radarrSettings = radarrSettings;
        _sonarrSettings = sonarrSettings;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Result> ExecuteAsync(NotifyArrAppsOnStartupCommand command, CancellationToken ct)
    {
        // Wipe stale sessions so Radarr/Sonarr are forced to re-authenticate immediately
        // rather than carrying over a SID from the previous process lifetime.
        await _authDbContext.DownloadClientSessions.ExecuteDeleteAsync(ct);

        // Only call testall when the arr app is actually configured — skip the HTTP round-trip
        // if the user has not set up the integration.
        if (_radarrSettings.IsConfigured && _radarrSettings.IsValidUrl() && _radarrSettings.IsValidApiKey())
            await TestAllAsync(_httpClientFactory.CreateRadarrHttpClient(), ct);

        if (_sonarrSettings.IsConfigured && _sonarrSettings.IsValidUrl() && _sonarrSettings.IsValidApiKey())
            await TestAllAsync(_httpClientFactory.CreateSonarrHttpClient(), ct);

        return Result.Ok();
    }

    /// <summary>
    /// Calls <c>POST /api/v3/downloadclient/testall</c> on the given arr app.
    /// </summary>
    /// <remarks>
    /// The endpoint returns <c>200 OK</c> when all clients pass or <c>400 Bad Request</c> when at
    /// least one fails. Either way, <c>RecordSuccess</c> is called for every client that passes,
    /// clearing its <c>DisabledTill</c> value and restoring it to active monitoring. Anything other
    /// than 200/400 (e.g. 502/503 — arr app unreachable) is logged as a warning but does not fail
    /// the overall startup sequence.
    /// </remarks>
    private static async Task TestAllAsync(HttpClient client, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri("/api/v3/downloadclient/testall", UriKind.Relative)
        );
        var result = await client.SendResultAsync(request, cancellationToken: ct);

        // 400 means some download clients failed their test, but the arr app still
        // called RecordSuccess for passing clients, which clears the DisabledTill
        // backoff that would otherwise block download monitoring after a restart.
        if (result.IsFailed && !result.Has400BadRequestError())
            result.ToResult().LogError();
    }
}
