using Newtonsoft.Json;
using PlexRipper.Application.Settings.Models.Base;

namespace PlexRipper.Application.Settings.Models
{
    public class AdvancedSettingsModel : BaseModel
    {
        private DownloadManagerModel _downloadManager = new();

        #region Properties

        [JsonProperty("downloadManager", Required = Required.Always)]
        public DownloadManagerModel DownloadManager
        {
            get => _downloadManager;
            set
            {
                if (value != null)
                {
                    _downloadManager = value;
                    _downloadManager.PropertyChanged += (_, _) => OnPropertyChanged();
                }
            }
        }

        #endregion Properties
    }
}