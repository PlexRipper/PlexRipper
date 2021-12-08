using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class GetDownloadTaskByMediaKeyQuery : IRequest<Result<DownloadTask>>
    {
        public GetDownloadTaskByMediaKeyQuery(int mediaId, PlexMediaType type)
        {
            MediaId = mediaId;
            Type = type;
        }
        public int MediaId { get; }

        public PlexMediaType Type { get; }

    }
}