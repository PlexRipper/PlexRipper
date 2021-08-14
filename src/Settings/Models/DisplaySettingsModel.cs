using System.Text.Json.Serialization;
using Newtonsoft.Json;
using PlexRipper.Application.Common;
using PlexRipper.Domain;
using PlexRipper.Settings.Models.Base;

namespace PlexRipper.Settings.Models
{
    public class DisplaySettingsModel : BaseModel, IDisplaySettingsModel
    {
        #region Properties

        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonProperty("tvShowViewMode", Required = Required.Always)]
        public ViewMode TvShowViewMode { get; set; } = ViewMode.Poster;

        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonProperty("movieViewMode", Required = Required.Always)]
        public ViewMode MovieViewMode { get; set; } = ViewMode.Poster;

        #endregion Properties
    }
}