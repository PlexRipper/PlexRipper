using Newtonsoft.Json;

namespace PlexRipper.Application.Common.DTO.Plex
{
    public class PlexAuthentication
    {
        [JsonProperty("user")]
        public PlexAccountDTO User { get; set; }
    }
}
