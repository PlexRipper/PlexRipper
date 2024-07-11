using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class GetPlexServersByIdsQuery : IRequest<Result<List<PlexServer>>>
{
    public GetPlexServersByIdsQuery(List<int> ids, bool includeConnections = false, bool includeLibraries = false)
    {
        Ids = ids;
        IncludeConnections = includeConnections;
        IncludeLibraries = includeLibraries;
    }

    public List<int> Ids { get; }

    public bool IncludeConnections { get; }

    public bool IncludeLibraries { get; }
}
