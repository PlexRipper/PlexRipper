using Microsoft.Extensions.DependencyInjection;
using TickerQ.DependencyInjection;
using TickerQ.Utilities.Enums;

namespace Reaparr.Application;

public static class BackgroundJobsRegistration
{
    public static void RegisterBackgroundJobs(this IServiceCollection services)
    {
        // TickerQ resolves the job from a new service scope for every execution.
        // Keep the job transient so a job instance (and its DbContext dependency)
        // can never be retained and reused after that execution scope is disposed.
        services.MapTicker<DownloadJob, DownloadTaskKey>(ServiceLifetime.Transient)
            .WithPriority(TickerTaskPriority.LongRunning)
            .WithMaxConcurrency(4);

        services.MapTicker<MoveDownloadFileJob, DownloadTaskKey>(ServiceLifetime.Transient)
            .WithPriority(TickerTaskPriority.LongRunning)
            .WithMaxConcurrency(1);

        services.MapTicker<InspectPlexServerJob, InspectPlexServerJobPayload>(ServiceLifetime.Transient)
            .WithPriority(TickerTaskPriority.LongRunning)
            .WithMaxConcurrency(1);

        services.MapTicker<MetadataSyncJob, MetadataSyncJobPayload>(ServiceLifetime.Transient)
            .WithPriority(TickerTaskPriority.Low)
            .WithMaxConcurrency(2);

        services.MapTicker<LibrarySyncJob, LibrarySyncJobPayload>(ServiceLifetime.Transient)
            .WithPriority(TickerTaskPriority.LongRunning)
            .WithMaxConcurrency(4);

        services.MapTicker<PlexLibraryComparisonJob, PlexLibraryComparisonJobPayload>(ServiceLifetime.Transient)
            .WithPriority(TickerTaskPriority.Low)
            .WithMaxConcurrency(2);

        services
            .MapTicker<CheckPlexLibrariesForUpdatesJob, CheckPlexLibrariesForUpdatesJobPayload>(ServiceLifetime
                .Transient)
            .WithPriority(TickerTaskPriority.Low)
            .WithCron("0 0 */3 * * *"); // Every 3 hours

        services.MapTicker<CheckForUpdateJob, CheckForUpdateJobPayload>(ServiceLifetime.Transient)
            .WithPriority(TickerTaskPriority.Low)
            .WithCron("0 0 * * * *");

        services
            .MapTicker<CheckAllConnectionsStatusByPlexServerJob, CheckAllConnectionsStatusByPlexServerJobPayload>(
                ServiceLifetime.Transient
            )
            .WithPriority(TickerTaskPriority.Low)
            .WithCron("0 */10 * * * *"); // Every 10 minutes
    }
}