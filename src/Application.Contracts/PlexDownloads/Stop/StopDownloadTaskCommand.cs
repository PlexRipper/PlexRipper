using FastEndpoints;
using FluentResults;

namespace Reaparr.Application.Contracts;

/// <summary>
/// Stops and disposes of the PlexDownloadClient executing the <see cref="DownloadTaskGeneric"/> if it is downloading.
/// </summary>
/// <param name="DownloadTaskGuid">The id of the <see cref="DownloadTaskGeneric"/> to stop.</param>
/// <param name="DeleteFiles">When true, deletes partially downloaded files.</param>
public record StopDownloadTaskCommand(Guid DownloadTaskGuid, bool DeleteFiles = true) : ICommand<Result>;
