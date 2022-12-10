using PlexRipper.Application;
using Quartz;

namespace BackgroundServices.InspectPlexServer;

public class InspectPlexServerJob : IJob
{
    public static string PlexServerIdParameter => "plexServerId";

    private readonly IPlexServerService _plexServerService;

    public InspectPlexServerJob(IPlexServerService plexServerService)
    {
        _plexServerService = plexServerService;
    }


    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var plexServerId = dataMap.GetIntValue(PlexServerIdParameter);
        Log.Debug($"Executing job: {nameof(InspectPlexServerJob)} for {nameof(plexServerId)}: {plexServerId}");
        await _plexServerService.InspectPlexServer(plexServerId);
    }

    public static JobKey GetJobKey(int id)
    {
        return new JobKey($"{PlexServerIdParameter}_{id}", nameof(InspectPlexServerJob));
    }
}