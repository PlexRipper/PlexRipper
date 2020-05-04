using Newtonsoft.Json;

namespace PlexRipper.Infrastructure.Common.Models.Plex
{
    public class PlexAuthentication
    {
        [JsonProperty("user")]
        public PlexAccountDTO User { get; set; }
    }
}
