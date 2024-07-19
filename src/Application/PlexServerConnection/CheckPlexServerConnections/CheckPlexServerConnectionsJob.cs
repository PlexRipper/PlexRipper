using System.Text.Json;
using Logging.Interface;
using Quartz;

namespace PlexRipper.Application;

public class CheckPlexServerConnectionsJob : IJob
{
    private readonly ILog _log;
    private readonly IMediator _mediator;

    public static string PlexServerIdsParameter => "plexServerIds";

    public static JobKey GetJobKey() => new(Guid.NewGuid().ToString(), nameof(CheckPlexServerConnectionsJob));

    public CheckPlexServerConnectionsJob(ILog log, IMediator mediator)
    {
        _log = log;
        _mediator = mediator;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var dataMap = context.JobDetail.JobDataMap;
            var cancellationToken = context.CancellationToken;
            var serializedIds = dataMap.GetString(PlexServerIdsParameter);
            if (serializedIds is null)
            {
                _log.WarningLine("No PlexServerIds found in job data map");
                return;
            }

            var plexServerIds = JsonSerializer.Deserialize<List<int>>(serializedIds);

            if (plexServerIds is null)
            {
                _log.WarningLine("Failed to deserialize PlexServerIds");
                return;
            }

            var serverTasks = plexServerIds.Select(async plexServerId =>
                await _mediator.Send(new CheckAllConnectionsStatusByPlexServerCommand(plexServerId), cancellationToken)
            );

            await Task.WhenAll(serverTasks);
        }
        catch (Exception e)
        {
            _log.Error(e);
        }
    }
}
