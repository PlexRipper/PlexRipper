using Quartz;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.BackgroundJobs;

/// <summary>
/// Quartz job that syncs detailed metadata (Parts/Streams) for a specific Plex server.
/// Runs after LibrarySyncJob completes and processes all media items that need enrichment.
/// </summary>
[DisallowConcurrentExecution]
public class MetadataSyncJob : IJob
{
    public const string ServerIdParameter = nameof(ServerIdParameter);

    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IReaparrDbContextFactory _dbContextFactory;

    public MetadataSyncJob(ILogger log, ICommandExecutor commandExecutor, IReaparrDbContextFactory dbContextFactory)
    {
        _log = log.ForContext<MetadataSyncJob>();
        _commandExecutor = commandExecutor;
        _dbContextFactory = dbContextFactory;
    }

    public static JobKey GetJobKey(int serverId) =>
        new($"{nameof(JobTypes.MetadataSyncJob)}_{serverId}", nameof(JobTypes.MetadataSyncJob));

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var ct = context.CancellationToken;

        if (!dataMap.ContainsKey(ServerIdParameter))
        {
            _log.Here().Error("Missing required parameter {Parameter} in job data map", ServerIdParameter);
            return;
        }

        var serverId = dataMap.GetInt(ServerIdParameter);

        // Use short-lived context for initial checks
        var dbContext = await _dbContextFactory.CreateAsync();
        var serverName = await dbContext.GetPlexServerNameById(serverId, cancellationToken: ct);
        var isServerOnline = await dbContext.IsServerOnline(serverId, ct);
        dbContext.Dispose();

        _log.Here().Information("Starting metadata sync for server {ServerName} ({ServerId})", serverName, serverId);

        if (!isServerOnline)
        {
            _log.Here()
                .Warning("Server {ServerName} ({ServerId}) is offline, skipping metadata sync", serverName, serverId);
            return;
        }

        try
        {
            var processedCount = 0;

            // Process movies
            var movieResult = await _commandExecutor.Send(new ProcessMovieMetadataCommand(serverId, serverName), ct);
            if (movieResult.IsSuccess)
            {
                processedCount += movieResult.Value;
            }

            // Process episodes
            var episodeResult = await _commandExecutor.Send(
                new ProcessEpisodeMetadataCommand(serverId, serverName),
                ct
            );
            if (episodeResult.IsSuccess)
            {
                processedCount += episodeResult.Value;
            }

            if (processedCount == 0)
            {
                _log.Here()
                    .Information(
                        "No media items need metadata enrichment for server {ServerName} ({ServerId})",
                        serverName,
                        serverId
                    );
            }
            else
            {
                _log.Here()
                    .Information(
                        "Completed metadata sync for server {ServerName} ({ServerId}). Processed {Count} items",
                        serverName,
                        serverId,
                        processedCount
                    );
            }
        }
        catch (OperationCanceledException)
        {
            _log.Here()
                .Information(
                    "Metadata sync for server {ServerName} ({ServerId}) has been cancelled",
                    serverName,
                    serverId
                );
        }
        catch (Exception e)
        {
            _log.Here().Error(e, "Failed to sync metadata for server {ServerName} ({ServerId})", serverName, serverId);
        }
    }
}
