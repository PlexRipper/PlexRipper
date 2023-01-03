using System.Text.Json.Serialization;
using PlexRipper.PlexApi.Converters;

namespace PlexRipper.PlexApi.Api;

// URL: {{SERVER_URL}}/library/sections?X-Plex-Token={{SERVER_TOKEN}}
public class LibrariesResponse
{
    [JsonPropertyName("MediaContainer")]
    public LibrariesResponseMediaContainer MediaContainer { get; set; }
}

public class LibrariesResponseMediaContainer
{
    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("allowSync")]
    public bool AllowSync { get; set; }

    [JsonPropertyName("title1")]
    public string Title1 { get; set; }

    [JsonPropertyName("Directory")]
    public List<LibrariesResponseDirectory> Directory { get; set; }
}

public class LibrariesResponseDirectory
{
    [JsonPropertyName("allowSync")]
    public bool AllowSync { get; set; }

    [JsonPropertyName("art")]
    public string Art { get; set; }

    [JsonPropertyName("composite")]
    public string Composite { get; set; }

    [JsonPropertyName("filters")]
    public bool Filters { get; set; }

    [JsonPropertyName("refreshing")]
    public bool Refreshing { get; set; }

    [JsonPropertyName("thumb")]
    public string Thumb { get; set; }

    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("agent")]
    public string Agent { get; set; }

    [JsonPropertyName("scanner")]
    public string Scanner { get; set; }

    [JsonPropertyName("language")]
    public string Language { get; set; }

    [JsonPropertyName("uuid")]
    public string Uuid { get; set; }

    [JsonPropertyName("updatedAt")]
    [JsonConverter(typeof(LongToDateTime))]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("createdAt")]
    [JsonConverter(typeof(LongToDateTime))]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("scannedAt")]
    [JsonConverter(typeof(LongToDateTime))]
    public DateTime ScannedAt { get; set; }

    [JsonPropertyName("content")]
    public bool Content { get; set; }

    [JsonPropertyName("directory")]
    public bool IsDirectory { get; set; }

    [JsonPropertyName("contentChangedAt")]
    [JsonConverter(typeof(LongToDateTime))]
    public DateTime ContentChangedAt { get; set; }

    [JsonPropertyName("hidden")]
    public int Hidden { get; set; }

    [JsonPropertyName("Location")]
    public List<LibrariesResponseLocation> Location { get; set; }
}

public class LibrariesResponseLocation
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("path")]
    public string Path { get; set; }
}