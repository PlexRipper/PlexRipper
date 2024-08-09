using Application.Contracts;
using Data.Contracts;
using Logging.Interface;
using Quartz;

namespace PlexRipper.Application;

public class InspectPlexServerJob : IJob
{
    public static string PlexServerIdParameter => "plexServerId";

    private readonly IPlexRipperDbContext _dbContext;
    private readonly IScheduler _scheduler;
    private readonly ISignalRService _signalRService;
    private readonly ILog _log;
    private readonly IMediator _mediator;

    public static JobKey GetJobKey(int id) => new($"{PlexServerIdParameter}_{id}", nameof(InspectPlexServerJob));

    public InspectPlexServerJob(
        ILog log,
        IMediator mediator,
        IPlexRipperDbContext dbContext,
        IScheduler scheduler,
        ISignalRService signalRService
    )
    {
        _log = log;
        _mediator = mediator;
        _dbContext = dbContext;
        _scheduler = scheduler;
        _signalRService = signalRService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var plexServerId = dataMap.GetIntValue(PlexServerIdParameter);
        _log.Debug(
            "Executing job: {InspectPlexServerJobName)} for {plexServerIdName)} with id: {PlexServerId}",
            nameof(InspectPlexServerJob),
            nameof(plexServerId),
            plexServerId
        );

        // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
        // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
        try
        {
            var refreshResult = await _mediator.Send(new RefreshPlexServerConnectionsCommand(plexServerId));
            if (refreshResult.IsFailed)
            {
                refreshResult.LogError();
                return;
            }

            // Create connection check task for all connections
            var checkJobKey = await _mediator.Send(
                new QueueCheckPlexServerConnectionsJobCommand([plexServerId]),
                CancellationToken.None
            );

            await _scheduler.AwaitJobRunning(checkJobKey, CancellationToken.None);

            // Refresh accessible libraries
            var accountsResult = await _dbContext.GetPlexAccountsWithAccessAsync(plexServerId);
            if (accountsResult.IsFailed)
                return;

            await Task.WhenAll(
                accountsResult.Value.Select(x => _mediator.Send(new RefreshLibraryAccessCommand(x.Id, plexServerId)))
            );

            // Notify front-end
            await _signalRService.SendRefreshNotificationAsync([DataType.PlexAccount, DataType.PlexLibrary]);

            // Sync library media
            await _mediator.Send(new QueueSyncServerJobCommand(plexServerId, true));

            _log.Information(
                "Successfully finished the inspection of {NameOfPlexServer} with id {PlexServerId}",
                nameof(PlexServer),
                plexServerId
            );
        }
        catch (Exception e)
        {
            _log.Error(e);
        }
    }
}
