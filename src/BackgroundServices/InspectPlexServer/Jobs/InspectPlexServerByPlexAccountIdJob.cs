using Application.Contracts;
using BackgroundServices.Contracts;
using Data.Contracts;
using Logging.Interface;
using Quartz;

namespace BackgroundServices.InspectPlexServer;

[PersistJobDataAfterExecution]
[DisallowConcurrentExecution]
public class InspectPlexServerByPlexAccountIdJob : IJob
{
    public static string PlexAccountIdParameter => "plexAccountId";

    private readonly ILog _log;
    private readonly IMediator _mediator;
    private readonly IPlexLibraryService _plexLibraryService;
    private readonly IPlexServerConnectionsService _plexServerConnectionsService;
    private readonly ISyncServerScheduler _syncServerScheduler;

    public InspectPlexServerByPlexAccountIdJob(
        ILog log,
        IMediator mediator,
        IPlexLibraryService plexLibraryService,
        IPlexServerConnectionsService plexServerConnectionsService,
        ISyncServerScheduler syncServerScheduler)
    {
        _log = log;
        _mediator = mediator;
        _plexLibraryService = plexLibraryService;
        _plexServerConnectionsService = plexServerConnectionsService;
        _syncServerScheduler = syncServerScheduler;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var plexAccountId = dataMap.GetIntValue(PlexAccountIdParameter);
        _log.Debug("Executing job: {InspectPlexServerByPlexAccountIdJob} for {PlexAccountIdName} with id: {PlexAccountId}",
            nameof(InspectPlexServerByPlexAccountIdJob),
            nameof(plexAccountId), plexAccountId);

        // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
        // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
        try
        {
            var plexAccountResult = await _mediator.Send(new GetPlexAccountByIdQuery(plexAccountId, true));
            if (plexAccountResult.IsFailed)
            {
                plexAccountResult.WithError($"Could not retrieve any PlexAccount from database with id {plexAccountId}.").LogError();
                return;
            }

            var plexAccount = plexAccountResult.Value;
            var plexServers = plexAccount.PlexServers;

            _log.Information("Inspecting {PlexServersCount} PlexServers for PlexAccount: {PlexAccountDisplayName}", plexServers.Count, plexAccount.DisplayName,
                0);

            // Check all connections of all Plex servers that this account has access to
            var checkResult = await _plexServerConnectionsService.CheckAllConnectionsOfPlexServersByAccountIdAsync(plexAccountId);
            if (checkResult.IsFailed)
                return;

            // Retrieve Libraries
            var libraryResult = await _plexLibraryService.RetrieveAllAccessibleLibrariesAsync(plexAccountId);
            if (libraryResult.IsFailed)
            {
                libraryResult.LogError();
                return;
            }

            // Sync libraries
            foreach (var plexServer in plexServers)
                await _syncServerScheduler.QueueSyncPlexServerJob(plexServer.Id, true);

            _log.Information(
                "Successfully finished the inspection of all plexServers related to {NameOfPlexAccount}: {PlexAccountDisplayName} with id: {PlexAccountId}",
                nameof(PlexAccount), plexAccount.DisplayName, plexAccountId);
        }
        catch (Exception e)
        {
            Log.Error(e);
        }
    }

    public static JobKey GetJobKey(int id)
    {
        return new JobKey($"{PlexAccountIdParameter}_{id}", nameof(InspectPlexServerByPlexAccountIdJob));
    }
}