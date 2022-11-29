using System.Text.Json.Serialization;
using PlexRipper.PlexApi.Helpers;

namespace PlexRipper.PlexApi.Models;

/// <summary>
/// Director Plex Object.
/// </summary>
public class Director
{
    [JsonPropertyName("id")]
    [JsonConverter(typeof(IntValueConverter))]
    public int Id { get; set; }

    [JsonPropertyName("filter")]
    public string Filter { get; set; }

    [JsonPropertyName("tag")]
    public string Tag { get; set; }
}