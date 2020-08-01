using System.Text.Json.Serialization;
using Plex.Api.Helpers;

namespace PlexRipper.PlexApi.Models
{
    public class Country
    {
        [JsonPropertyName("id")]
        [JsonConverter(typeof(IntValueConverter))]
        public int Id { get; set; }
        
        [JsonPropertyName("tag")]
        public string Tag { get; set; }
    }
}