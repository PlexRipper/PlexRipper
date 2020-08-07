using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class DownloadMovieDTO
    {
        [JsonProperty("plexAccountId", Required = Required.Always)]
        public int PlexAccountId { get; set; }

        [JsonProperty("plexMovieId", Required = Required.Always)]
        public int PlexMovieId { get; set; }
    }
}
