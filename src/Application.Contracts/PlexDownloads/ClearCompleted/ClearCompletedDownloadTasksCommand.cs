using FastEndpoints;
using FluentResults;

namespace Reaparr.Application.Contracts;

/// <summary>
/// Clears completed download tasks from the database.
/// </summary>
/// <returns>Returns total number of deleted rows.</returns>
public record ClearCompletedDownloadTasksCommand(List<Guid> DownloadTaskIds) : ICommand<Result<int>>;
