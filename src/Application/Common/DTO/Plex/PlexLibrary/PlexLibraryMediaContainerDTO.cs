using Newtonsoft.Json;
using System.Collections.Generic;

namespace PlexRipper.Application.Common.DTO.Plex.PlexLibrary
{
    public class PlexLibraryMediaContainerDTO
    {

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("allowSync")]
        public bool AllowSync { get; set; }

        [JsonProperty("art")]
        public string Art { get; set; }

        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        [JsonProperty("librarySectionID")]
        public int LibrarySectionID { get; set; }

        [JsonProperty("librarySectionTitle")]
        public string LibrarySectionTitle { get; set; }

        [JsonProperty("librarySectionUUID")]
        public string LibrarySectionUUID { get; set; }

        [JsonProperty("mediaTagPrefix")]
        public string MediaTagPrefix { get; set; }

        [JsonProperty("mediaTagVersion")]
        public int MediaTagVersion { get; set; }

        [JsonProperty("nocache")]
        public bool Nocache { get; set; }

        [JsonProperty("thumb")]
        public string Thumb { get; set; }

        [JsonProperty("title1")]
        public string Title1 { get; set; }

        [JsonProperty("title2")]
        public string Title2 { get; set; }

        [JsonProperty("viewGroup")]
        public string ViewGroup { get; set; }

        [JsonProperty("viewMode")]
        public int ViewMode { get; set; }

        [JsonProperty("Metadata")]
        public IList<PlexLibraryMetaData> Metadata { get; set; }

    }
}
