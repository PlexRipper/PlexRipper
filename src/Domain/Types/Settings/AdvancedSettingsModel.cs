using Newtonsoft.Json;

namespace PlexRipper.Domain.Settings
{
    public class AdvancedSettingsModel : BaseModel
    {

        #region Properties

        [JsonProperty("downloadManager", Required = Required.Always)]
        public DownloadManagerModel DownloadManager { get; set; } = new DownloadManagerModel();

        #endregion Properties

    }
}