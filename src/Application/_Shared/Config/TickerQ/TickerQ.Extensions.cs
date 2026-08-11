using Microsoft.Extensions.DependencyInjection;
using TickerQ.DependencyInjection;
using TickerQ.Utilities.Enums;

namespace Reaparr.Application;

public static class TickerQExtensions
{
    public static void RegisterBackgroundJobs(this IServiceCollection services)
    {
        // TickerQ resolves the job from a new service scope for every execution.
        // Keep the job transient so a job instance (and its DbContext dependency)
        // can never be retained and reused after that execution scope is disposed.
        services.MapTicker<DownloadJob, DownloadTaskKey>(ServiceLifetime.Transient)
            .WithMaxConcurrency(4)
            .WithPriority(TickerTaskPriority.LongRunning);

        services.MapTicker<MoveDownloadFileJob, DownloadTaskKey>(ServiceLifetime.Transient)
            .WithMaxConcurrency(1)
            .WithPriority(TickerTaskPriority.LongRunning);

        services.MapTicker<LibrarySyncJob, LibrarySyncJobPayload>(ServiceLifetime.Transient)
            .WithMaxConcurrency(4)
            .WithPriority(TickerTaskPriority.LongRunning);

        services.MapTicker<PlexLibraryComparisonJob, PlexLibraryComparisonJobPayload>(ServiceLifetime.Transient)
            .WithMaxConcurrency(2)
            .WithPriority(TickerTaskPriority.Low);

        services
            .MapTicker<CheckPlexLibrariesForUpdatesJob, CheckPlexLibrariesForUpdatesJobPayload>(ServiceLifetime
                .Transient)
            .WithPriority(TickerTaskPriority.Low)
            .WithCron("0 0 */3 * * *"); // Every 3 hours

        services.MapTicker<CheckForUpdateJob, CheckForUpdateJobPayload>(ServiceLifetime.Transient)
            .WithPriority(TickerTaskPriority.Low)
            .WithCron("0 0 * * * *");
    }
}