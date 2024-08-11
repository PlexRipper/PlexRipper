using System.Text.Json;
using Application.Contracts;
using Logging.Interface;
using Quartz;

namespace PlexRipper.Application;

public class CheckPlexServerConnectionsJob : IJob
{
    private readonly ILog _log;
    private readonly IMediator _mediator;
    private readonly ISignalRService _signalRService;

    public static string PlexServerIdsParameter => "plexServerIds";

    public static JobKey GetJobKey() => new(Guid.NewGuid().ToString(), nameof(CheckPlexServerConnectionsJob));

    public CheckPlexServerConnectionsJob(ILog log, IMediator mediator, ISignalRService signalRService)
    {
        _log = log;
        _mediator = mediator;
        _signalRService = signalRService;
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

            await _signalRService.SendRefreshNotificationAsync([DataType.PlexServerConnection], cancellationToken);
        }
        catch (Exception e)
        {
            _log.Error(e);
        }
    }
}
