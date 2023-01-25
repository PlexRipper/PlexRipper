using Application.Contracts;
using Logging.Interface;
using Quartz;

namespace BackgroundServices.InspectPlexServer;

public class InspectPlexServerJob : IJob
{
    public static string PlexServerIdParameter => "plexServerId";

    private readonly IPlexServerService _plexServerService;
    private readonly ILog _log;

    public InspectPlexServerJob(ILog log, IPlexServerService plexServerService)
    {
        _plexServerService = plexServerService;
        _log = log;
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
            await _plexServerService.InspectPlexServer(plexServerId);
        }
        catch (Exception e)
        {
            Log.Error(e);
        }
    }

    public static JobKey GetJobKey(int id)
    {
        return new JobKey($"{PlexServerIdParameter}_{id}", nameof(InspectPlexServerJob));
    }
}