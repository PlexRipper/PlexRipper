namespace Reaparr.Application;

public sealed record MetadataSyncJobPayload
{
    public required int ServerId { get; init; }
}

public sealed record MetadataSyncJobUpdateDTO
{
    public required int ServerId { get; init; }
}

/// <summary>
/// Syncs detailed metadata for a specific Plex server.
/// This job is registered but is not scheduled automatically.
/// </summary>
public class MetadataSyncJob : IJob
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IReaparrDbContextFactory _dbContextFactory;

    public MetadataSyncJob(ILogger log, ICommandExecutor commandExecutor, IReaparrDbContextFactory dbContextFactory)
    {
        _log = log.ForContext<MetadataSyncJob>();
        _commandExecutor = commandExecutor;
        _dbContextFactory = dbContextFactory;
    }

    public const string ServerIdParameter = nameof(ServerIdParameter);

    protected JobTypes JobType => JobTypes.MetadataSyncJob;

    protected List<RefreshDataType> RefreshDataTypes => [RefreshDataType.PlexLibrary];

    public static JobKey GetJobKey(int serverId) =>
        new($"{nameof(JobTypes.MetadataSyncJob)}_{serverId}", nameof(JobTypes.MetadataSyncJob));

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var cancellationToken = context.CancellationToken;

        if (!dataMap.ContainsKey(ServerIdParameter))
        {
            _log.Here().Error("Missing required parameter {Parameter} in job data map", ServerIdParameter);
            return;
        }

        var serverId = dataMap.GetInt(ServerIdParameter);
        using var dbContext = await _dbContextFactory.CreateAsync();
        var serverName = await dbContext.GetPlexServerNameById(serverId);
        var isServerOnline = await dbContext.IsServerOnline(serverId);

        _log.Here().Information("Starting metadata sync for server {ServerName} ({ServerId})", serverName, serverId);

        if (!isServerOnline)
        {
            _log.Here()
                .Warning("Server {ServerName} ({ServerId}) is offline, skipping metadata sync", serverName, serverId);
            return;
        }

        var processedCount = 0;
        var movieResult = await _commandExecutor.Send(
            new ProcessMovieMetadataCommand(serverId, serverName),
            cancellationToken
        );
        if (movieResult.IsSuccess)
            processedCount += movieResult.Value;

        var episodeResult = await _commandExecutor.Send(
            new ProcessEpisodeMetadataCommand(serverId, serverName),
            cancellationToken
        );
        if (episodeResult.IsSuccess)
            processedCount += episodeResult.Value;

        _log.Here()
            .Information(
                processedCount == 0
                    ? "No media items need metadata enrichment for server {ServerName} ({ServerId})"
                    : "Completed metadata sync for server {ServerName} ({ServerId}). Processed {ProcessedCount} items",
                serverName,
                serverId,
                processedCount
            );
    }
}
