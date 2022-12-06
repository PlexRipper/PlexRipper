namespace PlexRipper.Application;

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