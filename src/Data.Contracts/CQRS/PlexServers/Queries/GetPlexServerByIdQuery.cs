using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class GetPlexServerByIdQuery : IRequest<Result<PlexServer>>
{
    public GetPlexServerByIdQuery(int id, bool includeConnections = false, bool includeLibraries = false)
    {
        Id = id;
        IncludeConnections = includeConnections;
        IncludeLibraries = includeLibraries;
    }

    public int Id { get; }
    public bool IncludeConnections { get; }

    public bool IncludeLibraries { get; }
}