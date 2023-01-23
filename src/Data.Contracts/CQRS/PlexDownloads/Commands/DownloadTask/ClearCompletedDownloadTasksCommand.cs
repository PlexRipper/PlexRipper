using FluentResults;
using MediatR;

namespace Data.Contracts;

public class ClearCompletedDownloadTasksCommand : IRequest<Result>
{
    public List<int> DownloadTaskIds { get; }

    public ClearCompletedDownloadTasksCommand(List<int> downloadTaskIds = null)
    {
        DownloadTaskIds = downloadTaskIds;
    }
}