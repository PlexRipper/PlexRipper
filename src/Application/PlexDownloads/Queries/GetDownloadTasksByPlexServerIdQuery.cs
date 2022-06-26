using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class GetDownloadTasksByPlexServerIdQuery: IRequest<Result<List<DownloadTask>>>
    {
        public int PlexServerId { get; }

        public GetDownloadTasksByPlexServerIdQuery(int plexServerId)
        {
            PlexServerId = plexServerId;
        }
    }
}