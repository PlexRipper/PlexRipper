using Newtonsoft.Json;
using PlexRipper.Application.Settings.Models.Base;
using PlexRipper.Domain;

namespace PlexRipper.Application.Settings.Models
{
    public class DisplaySettingsModelDTO : BaseModel
    {
        #region Properties

        [JsonProperty("tvShowViewMode", Required = Required.Always)]
        public virtual ViewMode TvShowViewMode { get; set; } = ViewMode.Poster;

        [JsonProperty("movieViewMode", Required = Required.Always)]
        public virtual ViewMode MovieViewMode { get; set; } = ViewMode.Poster;

        #endregion Properties
    }
}