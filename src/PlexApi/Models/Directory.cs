using System.Text.Json.Serialization;
using PlexApi.Contracts;

namespace PlexRipper.PlexApi.Models;

public class Directory
{
    public bool AllowSync { get; set; }


    public string Art { get; set; }


    public string Composite { get; set; }


    public bool Filters { get; set; }


    public bool Refreshing { get; set; }


    public string Thumb { get; set; }


    public string Key { get; set; }


    public string Type { get; set; }


    public string Title { get; set; }


    public string Agent { get; set; }


    public string Scanner { get; set; }


    public string Language { get; set; }


    public string Uuid { get; set; }


    [JsonConverter(typeof(LongToDateTime))]
    public DateTime UpdatedAt { get; set; }


    [JsonConverter(typeof(LongToDateTime))]
    public DateTime CreatedAt { get; set; }


    [JsonConverter(typeof(LongToDateTime))]
    public DateTime ScannedAt { get; set; }


    public bool Content { get; set; }


    public bool IsDirectory { get; set; }


    public long ContentChangedAt { get; set; }


    public List<Location> Location { get; set; }
}