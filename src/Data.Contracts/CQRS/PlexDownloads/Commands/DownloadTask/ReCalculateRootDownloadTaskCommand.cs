using FluentResults;
using MediatR;

namespace Data.Contracts;

public class ReCalculateRootDownloadTaskCommand : IRequest<Result>
{
    public int RootDownloadTaskId { get; }

    public ReCalculateRootDownloadTaskCommand(int rootDownloadTaskId)
    {
        RootDownloadTaskId = rootDownloadTaskId;
    }
}