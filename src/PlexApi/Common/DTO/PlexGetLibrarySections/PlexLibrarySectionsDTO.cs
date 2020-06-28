using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Common.DTO.PlexGetLibrarySections
{
    /// <summary>
    /// Used in the GetLibrarySectionAsync Function
    /// </summary>
    public class PlexLibrarySectionsDTO
    {
        [JsonPropertyName("MediaContainer")]
        public PlexLibrarySectionsMediaContainerDTO MediaContainer { get; set; }
    }

    public class PlexLibrarySectionsMediaContainerDTO
    {

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("allowSync")]
        public bool AllowSync { get; set; }

        [JsonPropertyName("identifier")]
        public string Identifier { get; set; }

        [JsonPropertyName("mediaTagPrefix")]
        public string MediaTagPrefix { get; set; }

        [JsonPropertyName("mediaTagVersion")]
        public int MediaTagVersion { get; set; }

        [JsonPropertyName("title1")]
        public string Title1 { get; set; }

        [JsonPropertyName("Directory")]
        public IList<PlexLibrarySectionsDirectoryDTO> Directory { get; set; }
    }

    public class PlexLibrarySectionsDirectoryDTO
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
        public long UpdatedAt { get; set; }

        [JsonPropertyName("createdAt")]
        public long CreatedAt { get; set; }

        [JsonPropertyName("scannedAt")]
        public long ScannedAt { get; set; }

        [JsonPropertyName("content")]
        public bool Content { get; set; }

        [JsonPropertyName("directory")]
        public bool Directory { get; set; }

        [JsonPropertyName("contentChangedAt")]
        public long ContentChangedAt { get; set; }

        [JsonPropertyName("hidden")]
        public int Hidden { get; set; }

        [JsonPropertyName("Location")]
        public IList<PlexLibrarySectionsLocationDTO> Location { get; set; }
    }

    public class PlexLibrarySectionsLocationDTO
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }
    }
}
