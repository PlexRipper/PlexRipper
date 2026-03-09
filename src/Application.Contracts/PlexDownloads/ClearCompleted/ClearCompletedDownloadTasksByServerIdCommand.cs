using FastEndpoints;
using FluentResults;

namespace Reaparr.Application.Contracts;

/// <summary>
/// Clears all completed download tasks for a server from the database.
/// </summary>
/// <returns>Returns total number of deleted rows.</returns>
public record ClearCompletedDownloadTasksByServerIdCommand(int PlexServerId) : ICommand<Result<int>>;
