using System.Collections.Generic;
using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class PlexTvShowSeasonDTO : PlexMediaDTO
    {
        [JsonProperty("tvShowId", Required = Required.Always)]
        public int TvShowId { get; set; }

        [JsonProperty("episodes", Required = Required.Always)]
        public List<PlexTvShowEpisodeDTO> Episodes { get; set; }
    }
}