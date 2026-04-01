using FastEndpoints;
using FluentResults;

namespace Reaparr.Application.Contracts;

/// <summary>
/// Clears specific completed download tasks from the database by their keys.
/// </summary>
/// <returns>Returns total number of deleted rows.</returns>
public record ClearCompletedDownloadTasksByDownloadTaskKeyCommand(List<DownloadTaskKey> DownloadTaskKeys)
    : ICommand<Result<int>>;
