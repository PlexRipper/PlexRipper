using Newtonsoft.Json;
using PlexRipper.Application.Settings.Models.Base;
using PlexRipper.Domain;

namespace PlexRipper.Application.Settings.Models
{
    public class DisplaySettingsModel : BaseModel
    {
        #region Properties

        [JsonProperty("tvShowViewMode", Required = Required.Always)]
        public ViewMode TvShowViewMode { get; set; } = ViewMode.Poster;

        [JsonProperty("movieViewMode", Required = Required.Always)]
        public ViewMode MovieViewMode { get; set; } = ViewMode.Poster;

        #endregion Properties
    }
}