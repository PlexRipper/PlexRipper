namespace PlexRipper.Application
{
    public class CreatePlexServerStatusCommand : IRequest<Result<int>>
    {
        public PlexServerStatus PlexServerStatus { get; }

        public CreatePlexServerStatusCommand(PlexServerStatus plexServerStatus)
        {
            PlexServerStatus = plexServerStatus;
        }
    }
}