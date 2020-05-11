using Newtonsoft.Json;

namespace PlexRipper.Infrastructure.Common.DTO
{
    public class PlexAuthenticationDTO
    {
        [JsonProperty("user")]
        public PlexAccountDTO User { get; set; }
    }
}
