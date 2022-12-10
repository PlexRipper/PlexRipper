using PlexRipper.Application;
using Quartz;

namespace BackgroundServices.SyncServer;

public class SyncServerJob : IJob
{
    private readonly IPlexServerService _plexServerService;

    public static string PlexServerIdParameter => "plexServerId";

    public SyncServerJob(IPlexServerService plexServerService)
    {
        _plexServerService = plexServerService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var plexServerId = dataMap.GetIntValue(PlexServerIdParameter);
        Log.Debug($"Executing job: {nameof(SyncServerJob)} for {nameof(PlexServer)}: {plexServerId}");

        // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
        // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
        try
        {
            await _plexServerService.SyncPlexServer(plexServerId);
        }
        catch (Exception e)
        {
            Log.Error(e);
        }
    }
}