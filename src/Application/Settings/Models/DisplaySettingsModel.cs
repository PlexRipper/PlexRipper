using PlexRipper.Application.Settings.Models.Base;
using PlexRipper.Domain;

namespace PlexRipper.Application.Settings.Models
{
    public class DisplaySettingsModel : BaseModel
    {
        #region Properties

        public ViewMode TvShowViewMode { get; set; } = ViewMode.Poster;

        public ViewMode MovieViewMode { get; set; } = ViewMode.Poster;

        #endregion Properties
    }
}