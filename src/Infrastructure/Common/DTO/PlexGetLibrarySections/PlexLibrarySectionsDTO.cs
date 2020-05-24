using Newtonsoft.Json;
using PlexRipper.Domain.Extensions.Converters;
using System;
using System.Collections.Generic;

namespace PlexRipper.Infrastructure.Common.DTO.PlexGetLibrarySections
{
    /// <summary>
    /// Used in the GetLibrarySectionAsync Function
    /// </summary>
    public class PlexLibrarySectionsDTO
    {
        [JsonProperty("MediaContainer")]
        public PlexLibrarySectionsMediaContainerDTO MediaContainer { get; set; }
    }

    public class PlexLibrarySectionsMediaContainerDTO
    {

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("allowSync")]
        public bool AllowSync { get; set; }

        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        [JsonProperty("mediaTagPrefix")]
        public string MediaTagPrefix { get; set; }

        [JsonProperty("mediaTagVersion")]
        public int MediaTagVersion { get; set; }

        [JsonProperty("title1")]
        public string Title1 { get; set; }

        [JsonProperty("Directory")]
        public IList<PlexLibrarySectionsDirectoryDTO> Directory { get; set; }
    }

    public class PlexLibrarySectionsDirectoryDTO
    {

        [JsonProperty("allowSync")]
        public bool AllowSync { get; set; }

        [JsonProperty("art")]
        public string Art { get; set; }

        [JsonProperty("composite")]
        public string Composite { get; set; }

        [JsonProperty("filters")]
        public bool Filters { get; set; }

        [JsonProperty("refreshing")]
        public bool Refreshing { get; set; }

        [JsonProperty("thumb")]
        public string Thumb { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("agent")]
        public string Agent { get; set; }

        [JsonProperty("scanner")]
        public string Scanner { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }

        /// <summary>
        ///  Huge numbers might be returned, which is why the type is string which will later be safely converted by AutoMapper to a valid DateTime.
        /// </summary>
        [JsonProperty("updatedAt")]
        [JsonConverter(typeof(SafeUnixDateTimeConverter))]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        ///  Huge numbers might be returned, which is why the type is string which will later be safely converted by AutoMapper to a valid DateTime.
        /// </summary>
        [JsonProperty("createdAt")]
        [JsonConverter(typeof(SafeUnixDateTimeConverter))]
        public DateTime CreatedAt { get; set; }

        /// <summary>

        /// </summary>
        [JsonProperty("scannedAt")]
        [JsonConverter(typeof(SafeUnixDateTimeConverter))]
        public DateTime ScannedAt { get; set; }

        [JsonProperty("content")]
        public bool Content { get; set; }

        [JsonProperty("directory")]
        public bool Directory { get; set; }

        /// <summary>
        ///  Huge numbers might be returned, which is why the type is string which will later be safely converted by AutoMapper to a valid DateTime.
        /// </summary>
        [JsonProperty("contentChangedAt")]
        [JsonConverter(typeof(SafeUnixDateTimeConverter))]
        public DateTime ContentChangedAt { get; set; }

        [JsonProperty("hidden")]
        public int Hidden { get; set; }

        [JsonProperty("Location")]
        public IList<PlexLibrarySectionsLocationDTO> Location { get; set; }
    }

    public class PlexLibrarySectionsLocationDTO
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }
    }
}
