using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Models
{
    public class Subscription
    {
        [JsonPropertyName("active")]
        public bool Active { get; set; }
        
        [JsonPropertyName("status")]
        public string Status { get; set; }
        
        [JsonPropertyName("plan")]
        public object Plan { get; set; }
        
        [JsonPropertyName("features")]
        public object Features { get; set; }
    }
}