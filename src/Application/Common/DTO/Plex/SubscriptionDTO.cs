using Newtonsoft.Json;

namespace PlexRipper.Application.Common.DTO.Plex
{
    public class SubscriptionDTO
    {
        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("status")]
        public object Status { get; set; }

        [JsonProperty("plan")]
        public object Plan { get; set; }

        [JsonProperty("features")]
        public object Features { get; set; }
    }
}
