using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class GetDownloadTasksByPlexServerIdQuery : IRequest<Result<List<DownloadTask>>>
{
    public int PlexServerId { get; }

    public GetDownloadTasksByPlexServerIdQuery(int plexServerId)
    {
        PlexServerId = plexServerId;
    }
}