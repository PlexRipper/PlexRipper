using Application.Contracts;
using BackgroundServices.Contracts;
using Data.Contracts;
using Logging.Interface;
using Quartz;

namespace PlexRipper.Application;

public class InspectPlexServerJob : IJob
{
    public static string PlexServerIdParameter => "plexServerId";

    private readonly IPlexServerService _plexServerService;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IPlexLibraryService _plexLibraryService;
    private readonly ISyncServerScheduler _syncServerScheduler;
    private readonly IPlexServerConnectionsService _plexServerConnectionsService;
    private readonly ILog _log;
    private readonly IMediator _mediator;

    public InspectPlexServerJob(
        ILog log,
        IMediator mediator,
        IPlexServerService plexServerService,
        IPlexRipperDbContext dbContext,
        IPlexLibraryService plexLibraryService,
        ISyncServerScheduler syncServerScheduler,
        IPlexServerConnectionsService plexServerConnectionsService)
    {
        _log = log;
        _mediator = mediator;
        _plexServerService = plexServerService;
        _dbContext = dbContext;
        _plexLibraryService = plexLibraryService;
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

            var checkResult = await _plexServerConnectionsService.CheckAllConnectionsOfPlexServerAsync(plexServerId);
            if (checkResult.IsFailed)
            {
                checkResult.ToResult().LogError();
                return;
            }

            await _plexLibraryService.RetrieveAccessibleLibrariesForAllAccountsAsync(plexServerId);

            await _syncServerScheduler.QueueSyncPlexServerJob(plexServerId, true);

            _log.Information("Successfully finished the inspection of {NameOfPlexServer} with id {PlexServerId}", nameof(PlexServer), plexServerId);
        }
        catch (Exception e)
        {
            _log.Error(e);
        }
    }

    public static JobKey GetJobKey(int id)
    {
        return new JobKey($"{PlexServerIdParameter}_{id}", nameof(InspectPlexServerJob));
    }
}