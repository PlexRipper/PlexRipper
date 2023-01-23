using FluentResults;
using MediatR;

namespace Data.Contracts;

public class UpdateRootDownloadStatusOfDownloadTaskCommand : IRequest<Result>
{
    public int RootDownloadTaskId { get; }

    public UpdateRootDownloadStatusOfDownloadTaskCommand(int rootDownloadTaskId)
    {
        RootDownloadTaskId = rootDownloadTaskId;
    }
}