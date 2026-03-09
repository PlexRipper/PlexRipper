using FastEndpoints;
using FluentResults;

namespace Reaparr.Application.Contracts;

/// <summary>
/// Clears specific completed download tasks from the database by their IDs.
/// </summary>
/// <returns>Returns total number of deleted rows.</returns>
public record ClearCompletedDownloadTasksByDownloadTaskIdCommand(List<Guid> DownloadTaskIds) : ICommand<Result<int>>;
