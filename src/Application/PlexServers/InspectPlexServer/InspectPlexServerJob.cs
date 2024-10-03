using Application.Contracts;
using Data.Contracts;
using Logging.Interface;
using Quartz;

namespace PlexRipper.Application;

public class InspectPlexServerJob : IJob
{
    public static string PlexServerIdsParameter => "plexServerIds";

    private readonly IPlexRipperDbContext _dbContext;
    private readonly ISignalRService _signalRService;
    private readonly ILog _log;
    private readonly IMediator _mediator;

    public static JobKey GetJobKey() => new(Guid.NewGuid().ToString(), nameof(InspectPlexServerJob));

    public InspectPlexServerJob(
        ILog log,
        IMediator mediator,
        IPlexRipperDbContext dbContext,
        ISignalRService signalRService
    )
    {
        _log = log;
        _mediator = mediator;
        _dbContext = dbContext;
        _signalRService = signalRService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var cancellationToken = context.CancellationToken;

        var plexServerIds = dataMap.GetIntListValue(PlexServerIdsParameter);

        _log.Debug(
            "Executing job: {InspectPlexServerJobName} for {Count} servers",
            nameof(InspectPlexServerJob),
            plexServerIds.Count
        );

        try
        {
            var serverTasks = plexServerIds.Select(async plexServerId =>
            {
                // Check all Plex Server Connections
                await _mediator.Send(new CheckAllConnectionsStatusByPlexServerCommand(plexServerId), cancellationToken);
                await _signalRService.SendRefreshNotificationAsync([DataType.PlexServerConnection], cancellationToken);

                // Refresh and sync libraries
                await RefreshAndSyncLibraries(plexServerId, cancellationToken);
            });

            await Task.WhenAll(serverTasks);

            _log.Information("Successfully finished the inspection of {Count}", plexServerIds.Count, 0);
        }
        catch (Exception e)
        {
            // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
            // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
            _log.Error(e);
        }
    }

    private async Task<Result> RefreshAndSyncLibraries(int plexServerId, CancellationToken cancellationToken)
    {
        // Refresh accessible libraries
        var accountsResult = await _dbContext.GetPlexAccountsWithAccessAsync(plexServerId, cancellationToken);
        if (accountsResult.IsFailed)
            return accountsResult.ToResult().LogError();

        var plexAccountId = accountsResult.Value.First().Id;
        await _mediator.Send(new RefreshLibraryAccessCommand(plexAccountId, plexServerId), cancellationToken);

        // Notify front-end
        await _signalRService.SendRefreshNotificationAsync(
            [DataType.PlexAccount, DataType.PlexLibrary],
            CancellationToken.None
        );

        // Sync library media
        await _mediator.Send(new QueueSyncServerMediaJobCommand(plexServerId, true), CancellationToken.None);
        return Result.Ok();
    }
}
