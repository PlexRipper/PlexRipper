using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class GetAllPlexServerConnectionsQuery : IRequest<Result<List<PlexServerConnection>>>
{
    public GetAllPlexServerConnectionsQuery(bool includeServer = false, bool includeStatus = false)
    {
        IncludeServer = includeServer;
        IncludeStatus = includeStatus;
    }

    public bool IncludeServer { get; }
    public bool IncludeStatus { get; }
}