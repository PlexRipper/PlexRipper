﻿using Newtonsoft.Json;
using PlexRipper.Application.Settings.Models.Base;

namespace PlexRipper.Application.Settings.Models
{
    public class AdvancedSettingsModel : BaseModel
    {
        private DownloadManagerModel _downloadManager = new DownloadManagerModel();

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
                    _downloadManager.PropertyChanged += (sender, args) => OnPropertyChanged();
                }
            }
        }

        #endregion Properties
    }
}