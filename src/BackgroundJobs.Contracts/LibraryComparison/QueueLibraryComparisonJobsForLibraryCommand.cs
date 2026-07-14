using FastEndpoints;
using FluentResults;

namespace Reaparr.BackgroundJobs.Contracts;

/// <summary>
/// Requests comparison queue entries for every compatible remote-to-owned library pair affected by one library.
/// </summary>
/// <param name="PlexLibraryId">
/// The library whose sync, ownership, or access change should refresh comparison cache rows.
/// </param>
public record QueueLibraryComparisonJobsForLibraryCommand(int PlexLibraryId) : ICommand<Result>;
