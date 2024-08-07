using Data.Contracts;
using Logging.Interface;
using Quartz;

namespace PlexRipper.Application;

public class InspectPlexServerJob : IJob
{
    public static string PlexServerIdParameter => "plexServerId";

    private readonly IPlexRipperDbContext _dbContext;
    private readonly ILog _log;
    private readonly IMediator _mediator;

    public static JobKey GetJobKey(int id) => new($"{PlexServerIdParameter}_{id}", nameof(InspectPlexServerJob));

    public InspectPlexServerJob(ILog log, IMediator mediator, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _mediator = mediator;
        _dbContext = dbContext;
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

            var checkResult = await _mediator.Send(new CheckAllConnectionsStatusByPlexServerCommand(plexServerId));
            if (checkResult.IsFailed)
            {
                checkResult.ToResult().LogError();
                return;
            }

            await RefreshAccessibleLibraries(plexServerId);

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

    private async Task RefreshAccessibleLibraries(int plexServerId)
    {
        var accountsResult = await _dbContext.GetPlexAccountsWithAccessAsync(plexServerId);
        if (accountsResult.IsFailed)
            return;

        foreach (var plexAccount in accountsResult.Value)
            await _mediator.Send(new RefreshLibraryAccessCommand(plexAccount.Id, plexServerId));
    }
}
