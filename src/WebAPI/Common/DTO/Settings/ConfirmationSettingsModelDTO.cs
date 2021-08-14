using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class ConfirmationSettingsModelDTO
    {
        [JsonProperty(Required = Required.Always)]
        public bool AskDownloadMovieConfirmation { get; set; }

        [JsonProperty(Required = Required.Always)]
        public bool AskDownloadTvShowConfirmation { get; set; }

        [JsonProperty(Required = Required.Always)]
        public bool AskDownloadSeasonConfirmation { get; set; }

        [JsonProperty(Required = Required.Always)]
        public bool AskDownloadEpisodeConfirmation { get; set; }
    }
}