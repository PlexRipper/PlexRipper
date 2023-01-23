using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class GetPlexMediaIdByKeyQuery : IRequest<Result<int>>
{
    public DownloadTask DownloadTask { get; }

    public GetPlexMediaIdByKeyQuery(DownloadTask downloadTask)
    {
        DownloadTask = downloadTask;
    }
}