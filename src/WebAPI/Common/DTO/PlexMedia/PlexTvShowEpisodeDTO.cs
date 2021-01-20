using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class PlexTvShowEpisodeDTO : PlexMediaDTO
    {
        [JsonProperty("tvShowSeasonId", Required = Required.Always)]
        public int TvShowSeasonId { get; set; }
    }
}