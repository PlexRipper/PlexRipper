using Application.Contracts;
using BackgroundServices.Contracts;
using Data.Contracts;
using Logging.Interface;
using Quartz;

namespace PlexRipper.Application;

public class InspectPlexServerJob : IJob
{
    public static string PlexServerIdParameter => "plexServerId";

    private readonly IPlexRipperDbContext _dbContext;
    private readonly ISyncServerScheduler _syncServerScheduler;
    private readonly IPlexServerConnectionsService _plexServerConnectionsService;
    private readonly ILog _log;
    private readonly IMediator _mediator;

    public InspectPlexServerJob(
        ILog log,
        IMediator mediator,
        IPlexRipperDbContext dbContext,
        ISyncServerScheduler syncServerScheduler,
        IPlexServerConnectionsService plexServerConnectionsService)
    {
        _log = log;
        _mediator = mediator;
        _dbContext = dbContext;
        _plexServerConnectionsService = plexServerConnectionsService;
        _syncServerScheduler = syncServerScheduler;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var plexServerId = dataMap.GetIntValue(PlexServerIdParameter);
        _log.Debug("Executing job: {InspectPlexServerJobName)} for {plexServerIdName)} with id: {PlexServerId}", nameof(InspectPlexServerJob),
            nameof(plexServerId),
            plexServerId);

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

            var checkResult = await _mediator.Send(new CheckAllConnectionStatusCommand(plexServerId));
            if (checkResult.IsFailed)
            {
                checkResult.ToResult().LogError();
                return;
            }

            await RefreshAccessibleLibraries(plexServerId);

            await _syncServerScheduler.QueueSyncPlexServerJob(plexServerId, true);

            _log.Information("Successfully finished the inspection of {NameOfPlexServer} with id {PlexServerId}", nameof(PlexServer), plexServerId);
        }
        catch (Exception e)
        {
            _log.Error(e);
        }
    }

    private async Task RefreshAccessibleLibraries(int plexServerId)
    {
        var accountsResult = await _dbContext.GetPlexAccountsWithAccessAsync(plexServerId);
        if (accountsResult.IsFailed)
            return;

        foreach (var plexAccount in accountsResult.Value)
            await _mediator.Send(new RefreshLibraryAccessCommand(plexAccount.Id, plexServerId));
    }

    public static JobKey GetJobKey(int id)
    {
        return new JobKey($"{PlexServerIdParameter}_{id}", nameof(InspectPlexServerJob));
    }
}