namespace PlexRipper.Application.PlexAccounts;

public class GetPlexAccountsWithAccessByPlexServerIdQuery : IRequest<Result<List<PlexAccount>>>
{
    public GetPlexAccountsWithAccessByPlexServerIdQuery(int plexServerId)
    {
        PlexServerId = plexServerId;
    }

    public int PlexServerId { get; }
}