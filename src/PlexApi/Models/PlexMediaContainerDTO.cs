using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Models
{
    public class PlexMediaContainerDTO
    {
        [JsonPropertyName("MediaContainer")]
        public MediaContainer MediaContainer { get; set; }
    }
}