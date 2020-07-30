using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class DownloadMovieDTO
    {
        [JsonProperty("plexAccountId")]
        public int PlexAccountId { get; set; }

        [JsonProperty("plexMovieId")]
        public int PlexMovieId { get; set; }
    }
}
