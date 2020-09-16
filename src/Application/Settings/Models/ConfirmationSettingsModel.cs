using Newtonsoft.Json;
using PlexRipper.Application.Settings.Models.Base;

namespace PlexRipper.Application.Settings.Models
{
    public class ConfirmationSettingsModel : BaseModel
    {
        #region Properties

        public bool AskDownloadMovieConfirmation { get; set; } = true;

        public bool AskDownloadTvShowConfirmation { get; set; } = true;

        public bool AskDownloadSeasonConfirmation { get; set; } = true;

        public bool AskDownloadEpisodeConfirmation { get; set; } = true;

        #endregion Properties
    }
}