using Newtonsoft.Json;

namespace PlexRipper.Domain.Settings
{
    public class DownloadManagerModel : BaseModel
    {
        #region Fields

        private int _downloadSegments = 1;

        #endregion Fields

        #region Properties

        [JsonProperty("downloadSegments", Required = Required.Always)]
        public int DownloadSegments
        {
            get => _downloadSegments;
            set
            {
                if (value != _downloadSegments)
                {
                    _downloadSegments = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion Properties
    }
}
