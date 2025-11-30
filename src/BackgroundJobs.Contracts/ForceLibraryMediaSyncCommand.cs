using FastEndpoints;
using FluentResults;

namespace Reaparr.BackgroundJobs.Contracts;

/// <summary>
/// Command to move a library sync to the front of the queue by canceling the current execution and rescheduling with the highest priority.
/// </summary>
public record ForceLibraryMediaSyncCommand(int PlexServerId, int LibraryId) : ICommand<Result>;
