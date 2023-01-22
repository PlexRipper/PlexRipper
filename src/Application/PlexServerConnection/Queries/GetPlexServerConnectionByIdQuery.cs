namespace PlexRipper.Application;

public class GetPlexServerConnectionByIdQuery : IRequest<Result<PlexServerConnection>>
{
    public GetPlexServerConnectionByIdQuery(int id, bool includeServer = false, bool includeStatus = false)
    {
        Id = id;
        IncludeServer = includeServer;
        IncludeStatus = includeStatus;
    }

    public int Id { get; }

    public bool IncludeServer { get; }
    public bool IncludeStatus { get; }
}