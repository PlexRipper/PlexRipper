using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class GetPlexServerConnectionByPlexServerIdQuery : IRequest<Result<PlexServerConnection>>
{
    public GetPlexServerConnectionByPlexServerIdQuery(int plexServerId)
    {
        PlexServerId = plexServerId;
    }

    public int PlexServerId { get; }
}