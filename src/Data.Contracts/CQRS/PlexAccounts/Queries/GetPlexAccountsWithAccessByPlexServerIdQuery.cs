using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class GetPlexAccountsWithAccessByPlexServerIdQuery : IRequest<Result<List<PlexAccount>>>
{
    public GetPlexAccountsWithAccessByPlexServerIdQuery(int plexServerId)
    {
        PlexServerId = plexServerId;
    }

    public int PlexServerId { get; }
}