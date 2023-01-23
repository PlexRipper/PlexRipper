using Application.Contracts;
using Quartz;

namespace BackgroundServices.InspectPlexServer;

public class RefreshAccessiblePlexServersJob : IJob
{
    private readonly IPlexServerService _plexServerService;

    public static string PlexAccountIdParameter => "plexAccountId";

    public RefreshAccessiblePlexServersJob(IPlexServerService plexServerService)
    {
        _plexServerService = plexServerService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var plexAccountId = dataMap.GetIntValue(PlexAccountIdParameter);
        Log.Debug($"Executing job: {nameof(RefreshAccessiblePlexServersJob)} for {nameof(plexAccountId)}: {plexAccountId}");
        // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
        // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
        try
        {
            await _plexServerService.RefreshAccessiblePlexServersAsync(plexAccountId);
        }
        catch (Exception e)
        {
            Log.Error(e);
        }
    }

    public static JobKey GetJobKey(int id)
    {
        return new JobKey($"{PlexAccountIdParameter}_{id}", nameof(RefreshAccessiblePlexServersJob));
    }
}