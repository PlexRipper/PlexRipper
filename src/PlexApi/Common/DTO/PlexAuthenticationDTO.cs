using Newtonsoft.Json;

namespace PlexRipper.PlexApi.Common.DTO
{
    public class PlexAuthenticationDTO
    {
        [JsonProperty("user")]
        public PlexAccountDTO User { get; set; }
    }
}
