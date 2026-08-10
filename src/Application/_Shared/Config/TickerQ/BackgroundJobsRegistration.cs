using Microsoft.Extensions.DependencyInjection;
using TickerQ.DependencyInjection;
using TickerQ.Utilities.Enums;

namespace Reaparr.Application;

public static class BackgroundJobsRegistration
{
    public static void RegisterBackgroundJobs(this IServiceCollection services)
    {

        /*
         * TimeTicker Jobs - One off jobs
         */
        services.MapTicker<DownloadJob, DownloadTaskKey>(ServiceLifetime.Transient)
            .WithPriority(TickerTaskPriority.LongRunning);

        services.MapTicker<MoveDownloadFileJob, DownloadTaskKey>(ServiceLifetime.Transient)
            .WithPriority(TickerTaskPriority.LongRunning);

        services.MapTicker<InspectPlexServerJob, InspectPlexServerJobPayload>(ServiceLifetime.Transient)
            .WithPriority(TickerTaskPriority.LongRunning);

        services.MapTicker<LibrarySyncJob, LibrarySyncJobPayload>(ServiceLifetime.Transient)
            .WithPriority(TickerTaskPriority.LongRunning);

        services.MapTicker<MetadataSyncJob, MetadataSyncJobPayload>(ServiceLifetime.Transient)
            .WithPriority(TickerTaskPriority.Low)
            .WithMaxConcurrency(2);

        services.MapTicker<PlexLibraryComparisonJob, PlexLibraryComparisonJobPayload>(ServiceLifetime.Transient)
            .WithPriority(TickerTaskPriority.Low)
            .WithMaxConcurrency(1);

        /*
         * Cron Jobs
         * Note: Also update registration in BackgroundJobScheduler.SetupCronTickers()
         */
        services
            .MapTicker<CheckPlexLibrariesForUpdatesJob, CheckPlexLibrariesForUpdatesJobPayload>(ServiceLifetime
                .Transient)
            .WithPriority(TickerTaskPriority.Low);

        services.MapTicker<CheckForUpdateJob, CheckForUpdateJobPayload>(ServiceLifetime.Transient)
            .WithPriority(TickerTaskPriority.Low);

        services
            .MapTicker<CheckAllConnectionsStatusByPlexServerJob, CheckAllConnectionsStatusByPlexServerJobPayload>(
                ServiceLifetime.Transient
            )
            .WithPriority(TickerTaskPriority.Low);

        services.MapTicker<RefreshPlexAccountAccessJob, RefreshPlexAccountAccessJobPayload>(ServiceLifetime.Transient)
            .WithPriority(TickerTaskPriority.Low);
    }
}