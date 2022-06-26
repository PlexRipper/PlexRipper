using System.Text.Json.Serialization;
using PlexRipper.PlexApi.Helpers;

namespace PlexRipper.PlexApi.Models;

/// <summary>
/// 
/// </summary>
public class Writer
{
    [JsonConverter(typeof(IntValueConverter))]
    public int Id { get; set; }

    public string Filter { get; set; }

    public string Tag { get; set; }
}