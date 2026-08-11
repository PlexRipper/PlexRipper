using Microsoft.Extensions.DependencyInjection;
using TickerQ.DependencyInjection;

namespace Reaparr.Application;

public static class TickerQExtensions
{
    public static void RegisterBackgroundJobs(this IServiceCollection services)
    {
        // TickerQ resolves the job from a new service scope for every execution.
        // Keep the job transient so a job instance (and its DbContext dependency)
        // can never be retained and reused after that execution scope is disposed.
        services.MapTicker<LibrarySyncJob, LibrarySyncJobPayload>(ServiceLifetime.Transient);
        services.MapTicker<PlexLibraryComparisonJob, PlexLibraryComparisonJobPayload>(ServiceLifetime.Transient);
        services.MapTicker<CheckPlexLibrariesForUpdatesJob, CheckPlexLibrariesForUpdatesJobPayload>(
            ServiceLifetime.Transient
        ).WithCron("0 0 */3 * * *");
        services.MapTicker<CheckForUpdateJob, CheckForUpdateJobPayload>(ServiceLifetime.Transient)
            .WithCron("0 0 * * * *");
    }
}
