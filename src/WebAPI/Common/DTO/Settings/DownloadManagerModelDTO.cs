using Newtonsoft.Json;
using PlexRipper.Application.Settings.Models.Base;

namespace PlexRipper.Application.Settings.Models
{
    public class DownloadManagerModelDTO
    {
        #region Properties

        [JsonProperty("downloadSegments", Required = Required.Always)]
        public int DownloadSegments { get; set; }

        #endregion Properties
    }
}