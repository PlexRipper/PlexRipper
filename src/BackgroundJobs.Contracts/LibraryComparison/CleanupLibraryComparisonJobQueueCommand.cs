using FastEndpoints;
using FluentResults;

namespace Reaparr.BackgroundJobs.Contracts;

/// <summary>
/// Normalizes persisted library comparison queue rows during scheduler startup.
/// </summary>
/// <remarks>
/// Completed rows are disposable, while interrupted or failed rows are made queued again so shutdowns do not lose work.
/// </remarks>
public record CleanupLibraryComparisonJobQueueCommand : ICommand<Result>;
