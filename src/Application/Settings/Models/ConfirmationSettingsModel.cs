using Newtonsoft.Json;
using PlexRipper.Application.Settings.Models.Base;

namespace PlexRipper.Application.Settings.Models
{
    public class ConfirmationSettingsModel : BaseModel
    {
        #region Properties

        public virtual bool AskDownloadMovieConfirmation { get; set; } = true;

        public virtual bool AskDownloadTvShowConfirmation { get; set; } = true;

        public virtual bool AskDownloadSeasonConfirmation { get; set; } = true;

        public virtual bool AskDownloadEpisodeConfirmation { get; set; } = true;

        #endregion Properties
    }
}