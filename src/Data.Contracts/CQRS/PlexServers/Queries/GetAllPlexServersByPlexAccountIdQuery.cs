using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class GetAllPlexServersByPlexAccountIdQuery : IRequest<Result<List<PlexServer>>>
{
    public GetAllPlexServersByPlexAccountIdQuery(int plexAccountId)
    {
        PlexAccountId = plexAccountId;
    }

    public int PlexAccountId { get; }
}
