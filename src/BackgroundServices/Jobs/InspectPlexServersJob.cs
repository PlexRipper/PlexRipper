using PlexRipper.Application;
using Quartz;

namespace BackgroundServices.Jobs;

public class InspectPlexServersJob : IJob
{
    private readonly IPlexServerService _plexServerService;

    public InspectPlexServersJob(IPlexServerService plexServerService)
    {
        _plexServerService = plexServerService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var plexAccountId = dataMap.GetIntValue("plexAccountId");
        Log.Debug($"Executing job: {nameof(InspectPlexServersJob)} for plexAccountId: {plexAccountId}");
        await _plexServerService.InspectPlexServers(plexAccountId);
    }
}