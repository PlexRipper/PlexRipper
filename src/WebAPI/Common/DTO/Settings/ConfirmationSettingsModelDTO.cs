using Newtonsoft.Json;

namespace PlexRipper.Application.Settings.Models
{
    public class ConfirmationSettingsModelDTO
    {
        #region Properties

        [JsonProperty("askDownloadMovieConfirmation", Required = Required.Always)]
        public bool AskDownloadMovieConfirmation { get; set; }

        [JsonProperty("askDownloadTvShowConfirmation", Required = Required.Always)]
        public bool AskDownloadTvShowConfirmation { get; set; }

        [JsonProperty("askDownloadSeasonConfirmation", Required = Required.Always)]
        public bool AskDownloadSeasonConfirmation { get; set; }

        [JsonProperty("askDownloadEpisodeConfirmation", Required = Required.Always)]
        public bool AskDownloadEpisodeConfirmation { get; set; }

        #endregion Properties
    }
}