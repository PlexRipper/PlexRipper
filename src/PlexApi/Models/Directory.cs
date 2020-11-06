using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using PlexRipper.PlexApi.Config.Converters;

namespace PlexRipper.PlexApi.Models
{
    public class Directory
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
      
        
        [JsonPropertyName("Location")]
        public List<Location> Location { get; set; }
    }
}