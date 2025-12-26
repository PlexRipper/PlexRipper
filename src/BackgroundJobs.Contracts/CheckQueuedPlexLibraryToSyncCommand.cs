using FastEndpoints;
using FluentResults;

namespace Reaparr.BackgroundJobs.Contracts;

public record CheckQueuedPlexLibraryToSyncCommand : ICommand<Result>;
