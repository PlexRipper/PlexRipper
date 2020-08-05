using System.Text.Json.Serialization;
using PlexRipper.PlexApi.Helpers;

namespace PlexRipper.PlexApi.Models
{
    public class MediaRole
    {
        [JsonPropertyName("id")]
        [JsonConverter(typeof(IntValueConverter))]
        public int Id { get; set; }
        
        [JsonPropertyName("filter")]
        public string Filter { get; set; }
        
        [JsonPropertyName("role")]
        public string Role { get; set; }
        
        [JsonPropertyName("thumb")]
        public string Thumb { get; set; }
        
        [JsonPropertyName("tag")]
        public string Tag { get; set; }
    }
}