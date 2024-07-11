using System.Text.Json.Serialization;
using PlexRipper.PlexApi.Helpers;

namespace PlexRipper.PlexApi.Models;

public class Part
{
    // General

    [JsonConverter(typeof(IntValueConverter))]
    public int Id { get; set; }

    public string Key { get; set; }

    public int Duration { get; set; }

    public string File { get; set; }

    public long Size { get; set; }

    public string Container { get; set; }

    public string VideoProfile { get; set; }

    public Stream[] Stream { get; set; }

    public string AudioProfile { get; set; }

    public string HasThumbnail { get; set; }

    public string Indexes { get; set; }

    public bool? HasChapterTextStream { get; set; }
}
