using Application.Contracts;
using Logging.Interface;
using Quartz;

namespace BackgroundServices.InspectPlexServer;

public class RefreshAccessiblePlexServersJob : IJob
{
    private readonly ILog _log;
    private readonly IMediator _mediator;

    public static string PlexAccountIdParameter => "plexAccountId";

    public RefreshAccessiblePlexServersJob(ILog log, IMediator mediator)
    {
        _log = log;
        _mediator = mediator;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var plexAccountId = dataMap.GetIntValue(PlexAccountIdParameter);
        _log.Debug("Executing job: {NameOfRefreshAccessiblePlexServersJob)} for {NameOfPlexAccountId)} with id: {PlexAccountId}",
            nameof(RefreshAccessiblePlexServersJob),
            nameof(plexAccountId), plexAccountId);

        // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
        // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
        try
        {
            await _mediator.Send(new RefreshAccessiblePlexServersCommand(plexAccountId));
        }
        catch (Exception e)
        {
            _log.Error(e);
        }
    }

    public static JobKey GetJobKey(int id)
    {
        return new JobKey($"{PlexAccountIdParameter}_{id}", nameof(RefreshAccessiblePlexServersJob));
    }
}