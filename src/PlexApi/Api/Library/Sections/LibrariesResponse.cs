using System.Text.Json.Serialization;
using PlexApi.Contracts;

namespace PlexRipper.PlexApi.Api;

// URL: {{SERVER_URL}}/library/sections?X-Plex-Token={{SERVER_TOKEN}}
public class LibrariesResponse
{
    public LibrariesResponseMediaContainer MediaContainer { get; set; }
}

public class LibrariesResponseMediaContainer
{
    public int Size { get; set; }

    public bool AllowSync { get; set; }

    public string Title1 { get; set; }

    public List<LibrariesResponseDirectory> Directory { get; set; }
}

public class LibrariesResponseDirectory
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

    [JsonConverter(typeof(LongToDateTime))]
    public DateTime ContentChangedAt { get; set; }

    public int Hidden { get; set; }

    public List<LibrariesResponseLocation> Location { get; set; }
}

public class LibrariesResponseLocation
{
    public int Id { get; set; }

    public string Path { get; set; }
}
