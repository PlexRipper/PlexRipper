using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Models
{
    public class Collection
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }
        [JsonPropertyName("filter")]
        public string Filter { get; set; }
        [JsonPropertyName("tag")]
        public string Tag { get; set; }
    }
}