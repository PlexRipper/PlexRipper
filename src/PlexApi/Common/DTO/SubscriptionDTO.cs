using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Common.DTO
{
    public class SubscriptionDTO
    {
        [JsonPropertyName("active")]
        public bool Active { get; set; }

        [JsonPropertyName("status")]
        public object Status { get; set; }

        [JsonPropertyName("plan")]
        public object Plan { get; set; }

        [JsonPropertyName("features")]
        public object Features { get; set; }
    }
}
