using Newtonsoft.Json;
using PlexRipper.Application.Settings.Models.Base;

namespace PlexRipper.Application.Settings.Models
{
    public class ConfirmationSettingsModel : BaseModel
    {
        #region Properties

        [JsonProperty("askDownloadMovieConfirmation", Required = Required.Always)]
        public bool AskDownloadMovieConfirmation { get; set; } = true;

        [JsonProperty("askDownloadTvShowConfirmation", Required = Required.Always)]
        public bool AskDownloadTvShowConfirmation { get; set; } = true;

        [JsonProperty("askDownloadSeasonConfirmation", Required = Required.Always)]
        public bool AskDownloadSeasonConfirmation { get; set; } = true;

        [JsonProperty("askDownloadEpisodeConfirmation", Required = Required.Always)]
        public bool AskDownloadEpisodeConfirmation { get; set; } = true;

        #endregion Properties
    }
}