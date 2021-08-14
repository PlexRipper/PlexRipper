using Newtonsoft.Json;
using PlexRipper.Application.Common;
using PlexRipper.Settings.Models.Base;

namespace PlexRipper.Settings.Models
{
    public class AdvancedSettingsModel : BaseModel, IAdvancedSettingsModel
    {
        private DownloadManagerModel _downloadManager = new();

        #region Properties

        [JsonProperty("downloadManager", Required = Required.Always)]
        public IDownloadManagerModel DownloadManager
        {
            get => _downloadManager;
            set
            {
                if (value != null)
                {
                    _downloadManager = (DownloadManagerModel) value;
                    _downloadManager.PropertyChanged += (_, _) => OnPropertyChanged();
                }
            }
        }

        #endregion Properties
    }
}