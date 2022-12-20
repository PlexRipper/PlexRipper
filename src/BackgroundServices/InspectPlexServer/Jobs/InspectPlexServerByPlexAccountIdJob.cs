using PlexRipper.Application;
using Quartz;

namespace BackgroundServices.InspectPlexServer;

[PersistJobDataAfterExecution]
[DisallowConcurrentExecution]
public class InspectPlexServerByPlexAccountIdJob : IJob
{
    public static string PlexAccountIdParameter => "plexAccountId";

    private readonly IPlexServerService _plexServerService;

    public InspectPlexServerByPlexAccountIdJob(IPlexServerService plexServerService)
    {
        _plexServerService = plexServerService;
    }


    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var plexAccountId = dataMap.GetIntValue(PlexAccountIdParameter);
        Log.Debug($"Executing job: {nameof(InspectPlexServerByPlexAccountIdJob)} for {nameof(plexAccountId)}: {plexAccountId}");
        // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
        // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
        try
        {
            await _plexServerService.InspectPlexServers(plexAccountId, true);
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