using FastEndpoints;
using FluentResults;

namespace Reaparr.BackgroundJobs.Contracts;

/// <summary>
/// This is a command to clean up the library sync job queue by removing completed or failed jobs.
/// NOTE: This should only be run once on startup to ensure the queue is clean.
/// </summary>
public record CleanupLibrarySyncJobQueueCommand : ICommand<Result>;
