using Microsoft.EntityFrameworkCore;
using Quartz;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.PlexApi;
using Reaparr.PlexApi.Contracts;

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
    private readonly IReaparrDbContext _dbContext;

    public MetadataSyncJob(ILogger log, ICommandExecutor commandExecutor, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<MetadataSyncJob>();
        _commandExecutor = commandExecutor;
        _dbContext = dbContext;
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

        _log.Here().Information("Starting metadata sync for server {ServerId}", serverId);

        // Check server online
        var isServerOnline = await _dbContext.IsServerOnline(serverId, ct);
        if (!isServerOnline)
        {
            _log.Here().Warning("Server {ServerId} is offline, skipping metadata sync", serverId);
            return;
        }

        try
        {
            // Get media items needing metadata enrichment (missing Parts)
            var ratingKeys = await GetRatingKeysNeedingEnrichment(serverId, ct);

            if (ratingKeys.Length == 0)
            {
                _log.Here()
                    .Information("No media items need metadata enrichment for server {ServerId}", serverId);
                return;
            }

            _log.Here()
                .Information(
                    "Found {Count} media items needing metadata enrichment for server {ServerId}",
                    ratingKeys.Length,
                    serverId
                );

            // Process in batches
            const int batchSize = 50;
            var processedCount = 0;

            foreach (var batch in ratingKeys.Chunk(batchSize))
            {
                ct.ThrowIfCancellationRequested();

                var result = await _commandExecutor.Send(
                    new GetDetailMetadataByRatingKeysCommand(serverId, batch),
                    ct
                );

                if (result.IsFailed)
                {
                    _log.Here()
                        .Warning(
                            "Failed to fetch metadata batch for server {ServerId}: {Error}",
                            serverId,
                            result.Errors.FirstOrDefault()?.Message
                        );
                    continue;
                }

                // TODO: Persist the enriched metadata (Parts/Streams) to database
                // This will use the existing mappers and bulk insert
                processedCount += batch.Length;

                _log.Here()
                    .Debug(
                        "Processed {Processed}/{Total} media items for server {ServerId}",
                        processedCount,
                        ratingKeys.Length,
                        serverId
                    );
            }

            _log.Here()
                .Information(
                    "Completed metadata sync for server {ServerId}. Processed {Count} items",
                    serverId,
                    processedCount
                );
        }
        catch (OperationCanceledException)
        {
            _log.Here()
                .Information("Metadata sync for server {ServerId} has been cancelled", serverId);
        }
        catch (Exception e)
        {
            _log.Here().Error(e, "Failed to sync metadata for server {ServerId}", serverId);
        }
    }

    private async Task<string[]> GetRatingKeysNeedingEnrichment(int serverId, CancellationToken ct)
    {
        // Movies without MediaData.Parts - Key is the rating key used by Plex API
        var movieKeys = await _dbContext
            .PlexMovies.Where(m => m.PlexServerId == serverId)
            .Where(m => !m.MediaDataList.Any(md => md.Parts.Any()))
            .Select(m => m.Key.ToString())
            .ToArrayAsync(ct);

        // Episodes without MediaData.Parts
        var episodeKeys = await _dbContext
            .PlexTvShowEpisodes.Where(e => e.PlexServerId == serverId)
            .Where(e => !e.MediaDataList.Any(md => md.Parts.Any()))
            .Select(e => e.Key.ToString())
            .ToArrayAsync(ct);

        return [.. movieKeys, .. episodeKeys];
    }
}

