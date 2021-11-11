using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class GetAllDownloadTasksInPlexServerByIdQuery: IRequest<Result<List<DownloadTask>>>
    {
        public int PlexServerId { get; }

        public GetAllDownloadTasksInPlexServerByIdQuery(int plexServerId)
        {
            PlexServerId = plexServerId;
        }
    }
}