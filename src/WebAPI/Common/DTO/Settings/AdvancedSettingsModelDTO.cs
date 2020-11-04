using Newtonsoft.Json;

namespace PlexRipper.Application.Settings.Models
{
    public class AdvancedSettingsModelDTO
    {

        #region Properties

        [JsonProperty("downloadManager", Required = Required.Always)]
        public DownloadManagerModelDTO DownloadManager { get; set; } = new DownloadManagerModelDTO();

        #endregion Properties

    }
}