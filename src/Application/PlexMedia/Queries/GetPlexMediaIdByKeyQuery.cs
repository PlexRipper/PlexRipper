using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexMedia
{
    public class GetPlexMediaIdByKeyQuery : IRequest<Result<int>>
    {
        public DownloadTask DownloadTask { get; }

        public GetPlexMediaIdByKeyQuery(DownloadTask downloadTask)
        {
            DownloadTask = downloadTask;
        }
    }
}