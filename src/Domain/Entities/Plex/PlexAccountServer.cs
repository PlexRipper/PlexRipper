namespace PlexRipper.Domain.Entities.Plex
{
    public class PlexAccountServer
    {
        public long PlexAccountId { get; set; }
        public PlexAccount PlexAccount { get; set; }
        public int PlexServerId { get; set; }
        public PlexServer PlexServer { get; set; }
    }
}
