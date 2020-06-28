
namespace PlexRipper.PlexApi.Common.DTO.PlexGetServer
{
    public class PlexServerDTO
    {

        public string AccessToken { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public int Port { get; set; }

        public string Version { get; set; }

        public string Scheme { get; set; }

        public string Host { get; set; }

        public string LocalAddresses { get; set; }

        public string MachineIdentifier { get; set; }

        public long CreatedAt { get; set; }

        public long UpdatedAt { get; set; }

        public bool Owned { get; set; }

        public bool Synced { get; set; }

        public int OwnerId { get; set; }

        public bool Home { get; set; }
    }

}
