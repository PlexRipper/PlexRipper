using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Models
{
    public class Field
    {
        [JsonPropertyName("locked")]
        public bool Locked { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}