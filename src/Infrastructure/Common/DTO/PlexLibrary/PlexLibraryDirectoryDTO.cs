using Newtonsoft.Json;

namespace PlexRipper.Infrastructure.Common.DTO.PlexLibrary
{
    public class PlexLibraryDirectoryDTO
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
