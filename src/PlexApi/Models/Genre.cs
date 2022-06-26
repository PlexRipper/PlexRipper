using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Models;

public class Genre
{
    [JsonPropertyName("tag")]
    public string Tag { get; set; }
}