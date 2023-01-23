using FluentResults;
using MediatR;

namespace Data.Contracts;

public class DeleteDownloadWorkerTasksByDownloadTaskIdCommand : IRequest<Result>
{
    public int DownloadTaskId { get; }

    public DeleteDownloadWorkerTasksByDownloadTaskIdCommand(int downloadTaskId)
    {
        DownloadTaskId = downloadTaskId;
    }
}