using PlexRipper.Application;
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
        await _plexServerService.RefreshAccessiblePlexServersAsync(plexAccountId);
    }

    public static JobKey GetJobKey(int id)
    {
        return new JobKey($"{PlexAccountIdParameter}_{id}", nameof(RefreshAccessiblePlexServersJob));
    }
}