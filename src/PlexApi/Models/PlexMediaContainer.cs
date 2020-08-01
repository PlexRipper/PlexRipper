using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Models
{
    public class PlexMediaContainer
    {
        [JsonPropertyName("MediaContainer")]
        public MediaContainer MediaContainer { get; set; }
    }
}
