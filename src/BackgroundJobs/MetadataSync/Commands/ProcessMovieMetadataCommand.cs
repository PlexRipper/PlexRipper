using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Domain;
using Reaparr.PlexApi.Contracts;

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
                .PlexMovieDataParts.AsTracking()
                .Where(m => !m.HasMetadata && m.PlexServerId == command.ServerId)
                .Select(p => p.RatingKey)
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
                    .PlexMovieDataParts.AsTracking()
                    .Where(m => !m.HasMetadata && m.PlexServerId == command.ServerId)
                    .Where(p => batchRatingKeys.Contains(p.RatingKey))
                    .ToListAsync(ct);

                var ratingKeyGroups = parts.GroupBy(p => p.RatingKey).ToDictionary(g => g.Key, g => g.ToList());

                // Update entities with the enriched metadata
                foreach (var mediaItem in result.Value)
                {
                    if (!ratingKeyGroups.TryGetValue(mediaItem.RatingKey, out var partsToUpdate))
                    {
                        continue;
                    }

                    foreach (var part in partsToUpdate)
                    {
                        // Find the matching part DTO by PlexId
                        var partDto = mediaItem.Media.SelectMany(m => m.Parts).FirstOrDefault(p => p.Id == part.PlexId);
                        if (partDto == null)
                        {
                            _log.Here()
                                .Warning(
                                    "Part with PlexId {PlexId} (RatingKey {RatingKey}) not found in API response for server {ServerName} ({ServerId})",
                                    part.PlexId,
                                    part.RatingKey,
                                    command.ServerName,
                                    command.ServerId
                                );
                            skippedCount++;
                            continue;
                        }

                        part.UpdateMetadataFromDTO(partDto);
                        processedCount++;
                    }
                }

                // Handle parts for rating keys that weren't returned
                foreach (var missingRatingKey in missingRatingKeys)
                {
                    if (ratingKeyGroups.TryGetValue(missingRatingKey, out var partsToSkip))
                    {
                        skippedCount += partsToSkip.Count;
                    }
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
