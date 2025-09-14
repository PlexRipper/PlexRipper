using FastEndpoints;
using FluentResults;
using Reaparr.Domain;

namespace Reaparr.Application.Contracts;

/// <summary>
/// Generates a nested list of <see cref="DownloadTaskGeneric"/> and adds to the download queue.
/// </summary>
/// <returns>Returns true if all downloadTasks were added successfully.</returns>
public record CreateDownloadTasksCommand : ICommand<Result>
{
    public CreateDownloadTasksCommand(CreateDownloadTasksRequest request)
    {
        Request = request;
    }

    public CreateDownloadTasksCommand(List<DownloadMediaDTO> downloadMediaDtos)
    {
        Request = new CreateDownloadTasksRequest(downloadMediaDtos);
    }

    public CreateDownloadTasksRequest Request { get; }
}
