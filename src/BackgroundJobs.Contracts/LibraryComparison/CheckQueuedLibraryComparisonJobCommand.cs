namespace Reaparr.BackgroundJobs.Contracts;

/// <summary>
/// Starts or nudges the singleton library comparison queue worker when persisted queued work exists.
/// </summary>
public record CheckQueuedLibraryComparisonJobCommand : ICommand<Result>;