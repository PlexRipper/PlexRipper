namespace Reaparr.BackgroundJobs;

/// <summary>
/// Command to process movie metadata enrichment for a specific Plex server.
/// </summary>
/// <param name="ServerId">The ID of the Plex server to process metadata for.</param>
/// <param name="ServerName">The name of the Plex server for logging purposes.</param>
public record ProcessMovieMetadataCommand(int ServerId, string ServerName) : ICommand<Result<int>>;

public class ProcessMovieMetadataCommandValidator : AbstractValidator<ProcessMovieMetadataCommand>
{
    public ProcessMovieMetadataCommandValidator()
    {
        RuleFor(x => x.ServerId).GreaterThan(0);
        RuleFor(x => x.ServerName).NotEmpty();
    }
}

public class ProcessMovieMetadataCommandHandler : ICommandHandler<ProcessMovieMetadataCommand, Result<int>>
{
    private const int BATCH_SIZE = 50;
    private const int MAX_ITEMS_PER_RUN = 1000;

    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IReaparrDbContextFactory _dbContextFactory;

    public ProcessMovieMetadataCommandHandler(
        ILogger log,
        ICommandExecutor commandExecutor,
        IReaparrDbContextFactory dbContextFactory
    )
    {
        _log = log.ForContext<ProcessMovieMetadataCommandHandler>();
        _commandExecutor = commandExecutor;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Result<int>> ExecuteAsync(ProcessMovieMetadataCommand command, CancellationToken ct)
    {
        var processedCount = 0;
        var skippedCount = 0;

        // Fetch rating keys that need metadata enrichment using a short-lived context
        List<int> ratingKeysToProcess;
        using (var dbContext = await _dbContextFactory.CreateAsync())
        {
            ratingKeysToProcess = await dbContext
                .PlexMovieData.AsTracking()
                .Where(m =>
                    m.NeedsGeneratedName && m.PlexServerId == command.ServerId && m.GeneratedNameSyncedAt == null
                )
                .Select(p => p.PlexApiRatingKey)
                .Distinct()
                .Take(MAX_ITEMS_PER_RUN)
                .ToListAsync(ct);
        }

        if (ratingKeysToProcess.Count == 0)
            return Result.Ok(0);

        _log.Here()
            .Information(
                "Found {Count} Movie rating keys needing metadata enrichment for server {ServerName} ({ServerId})",
                ratingKeysToProcess.Count,
                command.ServerName,
                command.ServerId
            );

        // Process in batches, each with its own context
        foreach (var batch in ratingKeysToProcess.Select(k => k.ToString()).Chunk(BATCH_SIZE))
        {
            ct.ThrowIfCancellationRequested();

            var batchRatingKeys = batch.Select(int.Parse).ToHashSet();
            var result = await _commandExecutor.Send(
                new GetDetailMetadataByRatingKeysCommand(command.ServerId, batch),
                ct
            );

            if (result.IsFailed)
            {
                _log.Here()
                    .Warning(
                        "Failed to fetch Movie metadata batch for server {ServerName} ({ServerId}): {Error}",
                        command.ServerName,
                        command.ServerId,
                        result.Errors.FirstOrDefault()?.Message
                    );
                continue;
            }

            // Track which rating keys were returned
            var returnedRatingKeys = result.Value.Select(m => m.RatingKey).ToHashSet();
            var missingRatingKeys = batchRatingKeys.Except(returnedRatingKeys).ToList();

            if (missingRatingKeys.Count > 0)
            {
                _log.Here()
                    .Warning(
                        "Rating keys {RatingKeys} were requested but not returned for server {ServerName} ({ServerId}). Parts will be skipped.",
                        string.Join(", ", missingRatingKeys),
                        command.ServerName,
                        command.ServerId
                    );
            }

            // Create a new context for this batch to update entities
            using (var dbContext = await _dbContextFactory.CreateAsync())
            {
                // Re-fetch the parts for this batch with tracking enabled
                var parts = await dbContext
                    .PlexMovieData.AsTracking()
                    .Where(m =>
                        m.NeedsGeneratedName && m.PlexServerId == command.ServerId && m.GeneratedNameSyncedAt == null
                    )
                    .Where(p => batchRatingKeys.Contains(p.PlexApiRatingKey))
                    .ToListAsync(ct);

                // Update entities with the enriched metadata
                foreach (var metadataItem in result.Value)
                foreach (var mediaItem in metadataItem.Media)
                foreach (var partItem in mediaItem.Parts)
                {
                    var partToUpdate = parts.FirstOrDefault(p => p.PlexApiPartId == partItem.Id);

                    if (partToUpdate is null)
                    {
                        _log.Here()
                            .Warning(
                                "Part with PlexPartId {PlexPartId} (RatingKey {RatingKey}) not found in database for server {ServerName} ({ServerId})",
                                partItem.Id,
                                metadataItem.RatingKey,
                                command.ServerName,
                                command.ServerId
                            );
                        skippedCount++;
                        continue;
                    }

                    partToUpdate.UpdateStreamMetadata(metadataItem, mediaItem, partItem);
                    processedCount++;
                }

                await dbContext.SaveChangesAsync(ct);
            }

            _log.Here()
                .Debug(
                    "Batch complete: processed {Processed}, skipped {Skipped} Movie parts for server {ServerName} ({ServerId})",
                    processedCount,
                    skippedCount,
                    command.ServerName,
                    command.ServerId
                );
        }

        if (skippedCount > 0)
        {
            _log.Here()
                .Warning(
                    "Skipped {SkippedCount} Movie parts due to missing metadata for server {ServerName} ({ServerId})",
                    skippedCount,
                    command.ServerName,
                    command.ServerId
                );
        }

        return Result.Ok(processedCount);
    }
}
