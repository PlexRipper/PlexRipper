using System.Text.Json.Serialization;
using PlexRipper.PlexApi.Helpers;

namespace PlexRipper.PlexApi.Models;

public class Part
{
    // General
    [JsonPropertyName("id")]
    [JsonConverter(typeof(IntValueConverter))]
    public int Id { get; set; }

    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("file")]
    public string File { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("container")]
    public string Container { get; set; }

    [JsonPropertyName("videoProfile")]
    public string VideoProfile { get; set; }

    [JsonPropertyName("Stream")]
    public Stream[] Stream { get; set; }

    [JsonPropertyName("audioProfile")]
    public string AudioProfile { get; set; }

    [JsonPropertyName("hasThumbnail")]
    public string HasThumbnail { get; set; }

    [JsonPropertyName("indexes")]
    public string Indexes { get; set; }

    [JsonPropertyName("hasChapterTextStream")]
    public bool? HasChapterTextStream { get; set; }
}