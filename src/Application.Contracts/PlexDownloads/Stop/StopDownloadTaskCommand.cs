using FastEndpoints;
using FluentResults;
using Reaparr.Domain;

namespace Reaparr.Application.Contracts;

/// <summary>
/// Stops and disposes of the PlexDownloadClient executing the <see cref="DownloadTaskGeneric"/> if it is downloading.
/// </summary>
/// <param name="DownloadTaskGuid">The id of the <see cref="DownloadTaskGeneric"/> to stop.</param>
/// <returns>If successful a list of the DownloadTasks that were stopped.</returns>
public record StopDownloadTaskCommand(Guid DownloadTaskGuid) : ICommand<Result>;
