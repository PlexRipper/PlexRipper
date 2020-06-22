using PlexRipper.Application.Common.Interfaces.DownloadManager;
using PlexRipper.Application.Common.Interfaces.Settings;

namespace PlexRipper.Settings.Models
{
    public class DownloadManagerModel : BaseModel, IDownloadManagerModel
    {
        #region Fields

        private bool _confirmDelete = true;
        private string _downloadLocation = "/Downloads";
        private bool _enableSpeedLimit = false;
        private string _httpProxy = string.Empty;
        private bool _manualProxyConfig = false;
        private int _maxDownloads = 5;
        private int _memoryCacheSize = 1024;
        private string _proxyPassword = string.Empty;
        private int _proxyPort = 80;
        private string _proxyUsername = string.Empty;
        private int _speedLimit = 200;
        private bool _startDownloadsOnStartup = true;
        private string _tempDownloadLocation;

        #endregion Fields

        #region Properties

        public bool ConfirmDelete
        {
            get => _confirmDelete;
            set
            {
                if (value != _confirmDelete)
                {
                    _confirmDelete = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DownloadLocation
        {
            get => _downloadLocation;
            set
            {
                if (value != _downloadLocation)
                {
                    _downloadLocation = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TempDownloadLocation
        {
            get => _tempDownloadLocation;
            set
            {
                if (value != _tempDownloadLocation)
                {
                    _tempDownloadLocation = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool EnableSpeedLimit
        {
            get => _enableSpeedLimit;
            set
            {
                if (value != _enableSpeedLimit)
                {
                    _enableSpeedLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        public string HttpProxy
        {
            get => _httpProxy;
            set
            {
                if (value != _httpProxy)
                {
                    _httpProxy = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool ManualProxyConfig
        {
            get => _manualProxyConfig;
            set
            {
                if (value != _manualProxyConfig)
                {
                    _manualProxyConfig = value;
                    OnPropertyChanged();
                }
            }
        }

        public int MaxDownloads
        {
            get => _maxDownloads;
            set
            {
                if (value != _maxDownloads)
                {
                    _maxDownloads = value;
                    OnPropertyChanged();
                }
            }
        }
        public int MemoryCacheSize
        {
            get => _memoryCacheSize;
            set
            {
                if (value != _memoryCacheSize)
                {
                    _memoryCacheSize = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ProxyPassword
        {
            get => _proxyPassword;
            set
            {
                if (value != _proxyPassword)
                {
                    _proxyPassword = value;
                    OnPropertyChanged();
                }
            }
        }

        public int ProxyPort
        {
            get => _proxyPort;
            set
            {
                if (value != _proxyPort)
                {
                    _proxyPort = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ProxyUsername
        {
            get => _proxyUsername;
            set
            {
                if (value != _proxyUsername)
                {
                    _proxyUsername = value;
                    OnPropertyChanged();
                }
            }
        }

        public int SpeedLimit
        {
            get => _speedLimit;
            set
            {
                if (value != _speedLimit)
                {
                    _speedLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool StartDownloadsOnStartup
        {
            get => _startDownloadsOnStartup;
            set
            {
                if (value != _startDownloadsOnStartup)
                {
                    _startDownloadsOnStartup = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion Properties
    }
}
