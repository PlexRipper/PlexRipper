
using PlexRipper.Application.Common;

namespace PlexRipper.Settings.Models
{
    public class SettingsModel : BaseModel
    {
        #region Fields

        private bool _confirmExit = false;
        private IDownloadManagerModel _downloadManager = new DownloadManagerModel();
        private int _activeAccountId = 0;

        #endregion Fields

        #region Properties

        public int ActiveAccountId
        {
            get => _activeAccountId;
            set
            {
                if (value != _activeAccountId)
                {
                    _activeAccountId = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool ConfirmExit
        {
            get => _confirmExit;
            set
            {
                if (value != _confirmExit)
                {
                    _confirmExit = value;
                    OnPropertyChanged();
                }
            }
        }

        public IDownloadManagerModel DownloadManager
        {
            get => _downloadManager;
            set
            {
                if (value != _downloadManager)
                {
                    _downloadManager = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion Properties
    }
}
