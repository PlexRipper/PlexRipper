using System.Text.Json.Serialization;
using PlexRipper.PlexApi.Helpers;

namespace PlexRipper.PlexApi.Models;

/// <summary>
/// Director Plex Object.
/// </summary>
public class Director
{
    [JsonConverter(typeof(IntValueConverter))]
    public int Id { get; set; }

    public string Filter { get; set; }

    public string Tag { get; set; }
}