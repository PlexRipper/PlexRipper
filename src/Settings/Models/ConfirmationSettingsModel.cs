using Newtonsoft.Json;
using PlexRipper.Application.Common;
using PlexRipper.Settings.Models.Base;

namespace PlexRipper.Settings.Models
{
    public class ConfirmationSettingsModel : BaseModel, IConfirmationSettingsModel
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