using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class GetAllPlexServerConnectionsQuery : IRequest<Result<List<PlexServerConnection>>>
{
    public GetAllPlexServerConnectionsQuery(bool includeServer = false)
    {
        IncludeServer = includeServer;
    }

    public bool IncludeServer { get; }
}