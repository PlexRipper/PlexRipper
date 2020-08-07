using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class RefreshPlexLibraryDTO
    {
        [JsonProperty("plexAccountId", Required = Required.DisallowNull)]
        public int PlexAccountId { get; set; }

        [JsonProperty("plexLibraryId", Required = Required.DisallowNull)]
        public int PlexLibraryId { get; set; }
    }
}