using Data.Contracts;
using Logging.Interface;
using Quartz;

namespace PlexRipper.Application;

[DisallowConcurrentExecution]
public class CheckAllConnectionsStatusByPlexServerJob : IJob
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;

    public static JobKey GetJobKey() =>
        new(nameof(CheckAllConnectionsStatusByPlexServerJob), nameof(CheckAllConnectionsStatusByPlexServerJob));

    public CheckAllConnectionsStatusByPlexServerJob(ILog log, IPlexRipperDbContext dbContext, IMediator mediator)
    {
        _log = log;
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var cancellationToken = context.CancellationToken;
            var plexServerIds = _dbContext.PlexServers.Where(x => x.IsEnabled).Select(x => x.Id).ToList();

            if (!plexServerIds.Any())
            {
                return;
            }

            var serverTasks = plexServerIds.Select(async plexServerId =>
                await _mediator.Send(new CheckAllConnectionsStatusByPlexServerCommand(plexServerId), cancellationToken)
            );

            await Task.WhenAll(serverTasks);

            _log.Debug(
                "{JobName} for servers with ids: {PlexServerIds} completed",
                nameof(CheckAllConnectionsStatusByPlexServerJob),
                plexServerIds
            );
        }
        catch (Exception e)
        {
            _log.Error(e);
        }
    }
}
