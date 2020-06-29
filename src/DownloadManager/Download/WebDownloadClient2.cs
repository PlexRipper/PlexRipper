using PlexRipper.Application.Common.Interfaces.DownloadManager;
using PlexRipper.Application.Common.Interfaces.Settings;
using PlexRipper.DownloadManager.Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;

namespace PlexRipper.DownloadManager.Download
{
    public class WebDownloadClient2 : INotifyPropertyChanged
    {
        #region Fields

        // Used for blocking other processes when a file is being created or written to
        private static object fileLocker = new object();

        private readonly IDownloadManager _downloadManager;
        private readonly ILogger _logger;
        private readonly IUserSettings _userSettings;
        // List of download speed values in the last 10 seconds
        private List<int> downloadRates = new List<int>();

        private long lastNotificationDownloadedSize;

        // Time and size of downloaded data in the last calculaction of download speed
        private DateTime lastNotificationTime;

        // Time when the download was last started
        private DateTime lastStartTime;

        // Number format with a dot as the decimal separator
        private NumberFormatInfo numberFormat = NumberFormatInfo.InvariantInfo;

        // Average download speed in the last 10 seconds, used for calculating the time left to complete the download
        private int recentAverageRate;

        // Used for updating download speed on the DataGrid
        private int speedUpdateCount;

        // Download status
        private DownloadStatus status;

        #endregion Fields

        #region Methods

        // Calculate average download speed in the last 10 seconds
        private void CalculateAverageRate()
        {
            if (downloadRates.Count > 0)
            {
                if (downloadRates.Count > 10)
                    downloadRates.RemoveAt(0);

                int rateSum = 0;
                recentAverageRate = 0;
                foreach (int rate in downloadRates)
                {
                    rateSum += rate;
                }

                recentAverageRate = rateSum / downloadRates.Count;
            }
        }

        // Calculate download speed
        private void CalculateDownloadSpeed()
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan interval = now - lastNotificationTime;
            double timeDiff = interval.TotalSeconds;
            double sizeDiff = (double)(DownloadedSize + CachedSize - lastNotificationDownloadedSize);

            downloadSpeed = (int)Math.Floor(sizeDiff / timeDiff);

            downloadRates.Add(downloadSpeed);

            lastNotificationDownloadedSize = DownloadedSize + CachedSize;
            lastNotificationTime = now;
        }

        // Batch download URL check
        private void CheckBatchUrl()
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(this.Url);
            webRequest.Method = "HEAD";

            if (this.ServerLogin != null)
            {
                webRequest.PreAuthenticate = true;
                webRequest.Credentials = this.ServerLogin;
            }
            else
            {
                webRequest.Credentials = CredentialCache.DefaultCredentials;
            }

            if (_userSettings.DownloadManager.ManualProxyConfig && _userSettings.DownloadManager.HttpProxy != String.Empty)
            {
                this.Proxy = new WebProxy();
                this.Proxy.Address = new Uri("http://" + _userSettings.DownloadManager.HttpProxy + ":" + _userSettings.DownloadManager.ProxyPort);
                this.Proxy.BypassProxyOnLocal = false;
                if (_userSettings.DownloadManager.ProxyUsername != String.Empty && _userSettings.DownloadManager.ProxyPassword != String.Empty)
                {
                    this.Proxy.Credentials = new NetworkCredential(_userSettings.DownloadManager.ProxyUsername, _userSettings.DownloadManager.ProxyPassword);
                }
            }
            if (this.Proxy != null)
            {
                webRequest.Proxy = this.Proxy;
            }
            else
            {
                webRequest.Proxy = WebRequest.DefaultWebProxy;
            }

            using (WebResponse response = webRequest.GetResponse())
            {
                foreach (var header in response.Headers.AllKeys)
                {
                    if (header.Equals("Accept-Ranges", StringComparison.OrdinalIgnoreCase))
                    {
                        this.SupportsRange = true;
                    }
                }

                this.FileSize = response.ContentLength;

                if (this.FileSize <= 0)
                {
                    this.StatusString = "Error: The requested file does not exist";
                    this.FileSize = 0;
                    this.HasError = true;
                }

                RaisePropertyChanged("FileSizeString");
            }
        }

        // Create temporary file
        void CreateTempFile()
        {
            // Lock this block of code so other threads and processes don't interfere with file creation
            lock (fileLocker)
            {
                using (FileStream fileStream = File.Create(Directory.GetCurrentDirectory()))
                {
                    long createdSize = 0;
                    byte[] buffer = new byte[4096];
                    while (createdSize < this.FileSize)
                    {
                        int bufferSize = (this.FileSize - createdSize) < 4096
                            ? (int)(this.FileSize - createdSize) : 4096;
                        fileStream.Write(buffer, 0, bufferSize);
                        createdSize += bufferSize;
                    }
                }
            }
        }

        // Download file bytes from the HTTP response stream
        private void DownloadFile()
        {
            HttpWebRequest webRequest = null;
            HttpWebResponse webResponse = null;
            Stream responseStream = null;
            ThrottledStream throttledStream = null;
            MemoryStream downloadCache = null;
            speedUpdateCount = 0;
            recentAverageRate = 0;
            if (downloadRates.Count > 0)
                downloadRates.Clear();

            try
            {
                if (this.IsBatch && !this.BatchUrlChecked)
                {
                    CheckBatchUrl();
                    if (this.HasError)
                    {
                        this.RaiseDownloadCompleted();
                        return;
                    }
                    this.BatchUrlChecked = true;
                }

                //if (!TempFileCreated)
                //{
                //    // Reserve local disk space for the file
                //    CreateTempFile();
                //    this.TempFileCreated = true;
                //}

                this.lastStartTime = DateTime.UtcNow;

                if (this.Status == DownloadStatus.Waiting)
                    this.Status = DownloadStatus.Downloading;

                // Create request to the server to download the file
                webRequest = (HttpWebRequest)WebRequest.Create(this.Url);
                webRequest.Method = "GET";

                if (this.ServerLogin != null)
                {
                    webRequest.PreAuthenticate = true;
                    webRequest.Credentials = this.ServerLogin;
                }
                else
                {
                    webRequest.Credentials = CredentialCache.DefaultCredentials;
                }

                if (this.Proxy != null)
                {
                    webRequest.Proxy = this.Proxy;
                }
                else
                {
                    webRequest.Proxy = WebRequest.DefaultWebProxy;
                }

                // Set download starting point
                webRequest.AddRange(DownloadedSize);

                // Get response from the server and the response stream
                webResponse = (HttpWebResponse)webRequest.GetResponse();
                responseStream = webResponse.GetResponseStream();

                // Set a 5 second timeout, in case of internet connection break
                responseStream.ReadTimeout = 5000;

                // Set speed limit
                long maxBytesPerSecond = 0;
                if (_userSettings.DownloadManager.EnableSpeedLimit)
                {
                    maxBytesPerSecond = (long)((_userSettings.DownloadManager.SpeedLimit * 1024) / _downloadManager.ActiveDownloads);
                }
                else
                {
                    maxBytesPerSecond = ThrottledStream.Infinite;
                }
                throttledStream = new ThrottledStream(responseStream, maxBytesPerSecond);

                // Create memory cache with the specified size
                downloadCache = new MemoryStream(this.MaxCacheSize);

                // Create 1KB buffer
                byte[] downloadBuffer = new byte[this.BufferSize];

                int bytesSize = 0;
                CachedSize = 0;
                int receivedBufferCount = 0;

                // Download file bytes until the download is paused or completed
                while (true)
                {
                    if (SpeedLimitChanged)
                    {
                        if (_userSettings.DownloadManager.EnableSpeedLimit)
                        {
                            maxBytesPerSecond = (long)((_userSettings.DownloadManager.SpeedLimit * 1024) / _downloadManager.ActiveDownloads);
                        }
                        else
                        {
                            maxBytesPerSecond = ThrottledStream.Infinite;
                        }
                        throttledStream.MaximumBytesPerSecond = maxBytesPerSecond;
                        SpeedLimitChanged = false;
                    }

                    // Read data from the response stream and write it to the buffer
                    bytesSize = throttledStream.Read(downloadBuffer, 0, downloadBuffer.Length);

                    // If the cache is full or the download is paused or completed, write data from the cache to the temporary file
                    if (this.Status != DownloadStatus.Downloading || bytesSize == 0 || this.MaxCacheSize < CachedSize + bytesSize)
                    {
                        // Write data from the cache to the temporary file
                        WriteCacheToFile(downloadCache, CachedSize);

                        this.DownloadedSize += CachedSize;

                        // Reset the cache
                        downloadCache.Seek(0, SeekOrigin.Begin);
                        CachedSize = 0;

                        // Stop downloading the file if the download is paused or completed
                        if (this.Status != DownloadStatus.Downloading || bytesSize == 0)
                        {
                            break;
                        }
                    }

                    // Write data from the buffer to the cache
                    downloadCache.Write(downloadBuffer, 0, bytesSize);
                    CachedSize += bytesSize;

                    receivedBufferCount++;
                    if (receivedBufferCount == this.BufferCountPerNotification)
                    {
                        this.RaiseDownloadProgressChanged();
                        receivedBufferCount = 0;
                    }
                }

                // Update elapsed time when the download is paused or completed
                ElapsedTime = ElapsedTime.Add(DateTime.UtcNow - lastStartTime);

                // Change status
                if (this.Status != DownloadStatus.Deleting)
                {
                    if (this.Status == DownloadStatus.Pausing)
                    {
                        this.Status = DownloadStatus.Paused;
                        UpdateDownloadDisplay();
                    }
                    else if (this.Status == DownloadStatus.Queued)
                    {
                        UpdateDownloadDisplay();
                    }
                    else
                    {
                        this.CompletedOn = DateTime.UtcNow;
                        this.RaiseDownloadCompleted();
                    }
                }
            }
            catch (Exception ex)
            {
                // Show error in the status
                this.StatusString = "Error: " + ex.Message;
                this.HasError = true;
                this.RaiseDownloadCompleted();
            }
            finally
            {
                // Close the response stream and cache, stop the thread
                responseStream?.Close();
                //throttledStream?.Close();
                webResponse?.Close();
                downloadCache?.Close();
                // DownloadThread?.Abort();
            }
        }

        // Reset download properties to default values
        private void ResetProperties()
        {
            HasError = false;
            TempFileCreated = false;
            DownloadedSize = 0;
            CachedSize = 0;
            speedUpdateCount = 0;
            recentAverageRate = 0;
            downloadRates.Clear();
            ElapsedTime = new TimeSpan();
            CompletedOn = DateTime.MinValue;
        }

        // Update download display (on downloadsGrid and propertiesGrid controls)
        private void UpdateDownloadDisplay()
        {
            RaisePropertyChanged("DownloadedSizeString");
            RaisePropertyChanged("PercentString");
            RaisePropertyChanged("Progress");

            // New download speed update every 4 seconds
            TimeSpan startInterval = DateTime.UtcNow - lastStartTime;
            if (speedUpdateCount == 0 || startInterval.TotalSeconds < 4 || this.HasError || this.Status == DownloadStatus.Paused
                || this.Status == DownloadStatus.Queued || this.Status == DownloadStatus.Completed)
            {
                RaisePropertyChanged("DownloadSpeed");
            }
            speedUpdateCount++;
            if (speedUpdateCount == 4)
                speedUpdateCount = 0;

            RaisePropertyChanged("TimeLeft");
            RaisePropertyChanged("StatusString");
            RaisePropertyChanged("CompletedOnString");

            if (this.IsSelected)
            {
                RaisePropertyChanged("AverageSpeedAndTotalTime");
            }
        }

        // Write data from the cache to the temporary file
        void WriteCacheToFile(MemoryStream downloadCache, int cachedSize)
        {
            // Block other threads and processes from using the file
            lock (fileLocker)
            {
                using (FileStream fileStream = new FileStream(TempDownloadPath, FileMode.Open))
                {
                    byte[] cacheContent = new byte[cachedSize];
                    downloadCache.Seek(0, SeekOrigin.Begin);
                    downloadCache.Read(cacheContent, 0, cachedSize);
                    fileStream.Seek(DownloadedSize, SeekOrigin.Begin);
                    fileStream.Write(cacheContent, 0, cachedSize);
                }
            }
        }

        // Generate DownloadCompleted event
        protected virtual void RaiseDownloadCompleted()
        {
            if (DownloadFileCompleted != null)
            {
                DownloadFileCompleted(this, EventArgs.Empty);
            }
        }

        // Generate DownloadProgressChanged event
        protected virtual void RaiseDownloadProgressChanged()
        {
            Log.Information(PercentString);
            if (DownloadProgressChanged != null)
            {

            }
        }

        // Generate PropertyChanged event to update the UI
        protected void RaisePropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        // Generate StatusChanged event
        protected virtual void RaiseStatusChanged()
        {
            StatusChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion Methods

        // Download speed
        public int downloadSpeed;

        // Thread for the download process
        public Thread DownloadThread = null;

        // Elapsed time (doesn't include the time period when the download was paused)
        public TimeSpan ElapsedTime = new TimeSpan();

        // HTTP proxy server information
        public WebProxy Proxy = null;

        // Username and password for accessing the HTTP server
        public NetworkCredential ServerLogin = null;

        // Status text in the DataGrid
        public string StatusText;

        // Date and time when the download was added to the list
        public DateTime AddedOn { get; set; }

        public string AddedOnString
        {
            get
            {
                string format = "dd.MM.yyyy. HH:mm:ss";
                return AddedOn.ToString(format);
            }
        }

        // Average download speed
        public string AverageDownloadSpeed => DataFormat.FormatSpeedString((int)Math.Floor((double)(DownloadedSize + CachedSize) / TotalElapsedTime.TotalSeconds));

        // Batch URL was checked
        public bool BatchUrlChecked { get; set; }

        // Download buffer count per notification (DownloadProgressChanged event)
        public int BufferCountPerNotification { get; set; }

        // Buffer size
        public int BufferSize { get; set; }

        // Size of downloaded data in the cache memory
        public int CachedSize { get; set; }

        // Date and time when the download was completed
        public DateTime CompletedOn { get; set; }

        public string CompletedOnString
        {
            get
            {
                if (CompletedOn != DateTime.MinValue)
                {
                    string format = "dd.MM.yyyy. HH:mm:ss";
                    return CompletedOn.ToString(format);
                }
                else
                    return String.Empty;
            }
        }

        // Size of downloaded data which was written to the local file
        public long DownloadedSize { get; set; }

        public string DownloadedSizeString => DataFormat.FormatSizeString(DownloadedSize + CachedSize);

        // Local folder which contains the file
        public string DownloadFolder => this.TempDownloadPath.Remove(TempDownloadPath.LastIndexOf("\\") + 1);

        // Downloaded file path
        public string DownloadPath => this.TempDownloadPath.Remove(this.TempDownloadPath.Length - 4);

        public string DownloadSpeed
        {
            get
            {
                if (this.Status == DownloadStatus.Downloading && !this.HasError)
                {
                    return DataFormat.FormatSpeedString(downloadSpeed);
                }
                return String.Empty;
            }
        }

        // File name
        public string FileName { get; set; }

        // File size (in bytes)
        public long FileSize { get; set; }

        public string FileSizeString => DataFormat.FormatSizeString(FileSize);

        // File type (extension)
        public string FileType => Url.ToString().Substring(Url.ToString().LastIndexOf('.') + 1).ToUpper();

        // There was an error during download
        public bool HasError { get; set; }

        // Download is part of a batch
        public bool IsBatch { get; set; }

        // Download is selected in the DataGrid
        public bool IsSelected { get; set; }

        // Last update time of the DataGrid item
        public DateTime LastUpdateTime { get; set; }

        public ILogger Log => _logger;

        // Maxiumum cache size
        public int MaxCacheSize { get; set; }

        // Open file as soon as the download is completed
        public bool OpenFileOnCompletion { get; set; }

        // Percentage of downloaded data
        public float Percent => ((float)(DownloadedSize + CachedSize) / (float)FileSize) * 100F;

        public string PercentString
        {
            get
            {
                if (Percent < 0 || float.IsNaN(Percent))
                    return "0.0%";
                else if (Percent > 100)
                    return "100.0%";
                else
                    return String.Format(numberFormat, "{0:0.0}%", Percent);
            }
        }

        // Speed limit was changed
        public bool SpeedLimitChanged { get; set; }

        public DownloadStatus Status
        {
            get => status;
            set
            {
                status = value;
                if (status != DownloadStatus.Deleting)
                    RaiseStatusChanged();
            }
        }

        public string StatusString
        {
            get
            {
                if (this.HasError)
                    return StatusText;
                else
                    return Status.ToString();
            }
            set
            {
                StatusText = value;
                RaiseStatusChanged();
            }
        }

        // Server supports the Range header (resuming the download)
        public bool SupportsRange { get; set; }

        // Temporary file path
        public string TempDownloadPath { get; set; }

        // Temporary file was created
        public bool TempFileCreated { get; set; }

        // Time left to complete the download
        public string TimeLeft
        {
            get
            {
                if (recentAverageRate > 0 && this.Status == DownloadStatus.Downloading && !this.HasError)
                {
                    double secondsLeft = (FileSize - DownloadedSize + CachedSize) / recentAverageRate;

                    TimeSpan span = TimeSpan.FromSeconds(secondsLeft);

                    return DataFormat.FormatTimeSpanString(span);
                }
                return String.Empty;
            }
        }

        // Total elapsed time (includes the time period when the download was paused)
        public TimeSpan TotalElapsedTime
        {
            get
            {
                if (this.Status != DownloadStatus.Downloading)
                {
                    return ElapsedTime;
                }
                else
                {
                    return ElapsedTime.Add(DateTime.UtcNow - lastStartTime);
                }
            }
        }

        public string TotalElapsedTimeString => DataFormat.FormatTimeSpanString(TotalElapsedTime);

        // URL of the file to download
        public Uri Url { get; private set; }

        public WebDownloadClient2(IDownloadManager downloadManager, IUserSettings userSettings)
        {
            _downloadManager = downloadManager;
            _userSettings = userSettings;

            this.BufferSize = 1024; // Buffer size is 1KB
            this.MaxCacheSize = _userSettings.DownloadManager.MemoryCacheSize * 1024; // Default cache size is 1MB
            this.BufferCountPerNotification = 64;



            this.Status = DownloadStatus.Initialized;
        }
        public event EventHandler DownloadFileCompleted;

        public event DownloadProgressChangedEventHandler DownloadProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler StatusChanged;
        // Check URL to get file size, set login and/or proxy server information, check if the server supports the Range header
        public void CheckUrl()
        {
            try
            {
                var webRequest = (HttpWebRequest)WebRequest.Create(this.Url);
                webRequest.Method = "HEAD";
                webRequest.Timeout = 5000;

                if (this.ServerLogin != null)
                {
                    webRequest.PreAuthenticate = true;
                    webRequest.Credentials = this.ServerLogin;
                }
                else
                {
                    webRequest.Credentials = CredentialCache.DefaultCredentials;
                }

                if (_userSettings.DownloadManager.ManualProxyConfig && _userSettings.DownloadManager.HttpProxy != String.Empty)
                {
                    this.Proxy = new WebProxy();
                    this.Proxy.Address = new Uri("http://" + _userSettings.DownloadManager.HttpProxy + ":" + _userSettings.DownloadManager.ProxyPort);
                    this.Proxy.BypassProxyOnLocal = false;
                    if (_userSettings.DownloadManager.ProxyUsername != String.Empty && _userSettings.DownloadManager.ProxyPassword != String.Empty)
                    {
                        this.Proxy.Credentials = new NetworkCredential(_userSettings.DownloadManager.ProxyUsername, _userSettings.DownloadManager.ProxyPassword);
                    }
                }
                if (this.Proxy != null)
                {
                    webRequest.Proxy = this.Proxy;
                }
                else
                {
                    webRequest.Proxy = WebRequest.DefaultWebProxy;
                }

                using (WebResponse response = webRequest.GetResponse())
                {
                    foreach (var header in response.Headers.AllKeys)
                    {
                        if (header.Equals("Accept-Ranges", StringComparison.OrdinalIgnoreCase))
                        {
                            this.SupportsRange = true;
                        }
                    }

                    this.FileSize = response.ContentLength;

                    if (this.FileSize <= 0)
                    {
                        Log.Error("The requested file does not exist!");
                        this.HasError = true;
                    }
                }
            }
            catch (Exception)
            {
                Log.Error("There was an error while getting the file information. Please make sure the URL is accessible.");
                this.HasError = true;
            }
        }

        // DownloadCompleted event handler
        public void DownloadCompletedHandler(object sender, EventArgs e)
        {
            if (!this.HasError)
            {
                // If the file already exists, delete it
                if (File.Exists(this.DownloadPath))
                {
                    File.Delete(this.DownloadPath);
                }

                // Convert the temporary (.tmp) file to the actual (requested) file
                if (File.Exists(this.TempDownloadPath))
                {
                    File.Move(this.TempDownloadPath, this.DownloadPath);
                }

                this.Status = DownloadStatus.Completed;
                UpdateDownloadDisplay();

                if (this.OpenFileOnCompletion && File.Exists(this.DownloadPath))
                {
                    Process.Start(@DownloadPath);
                }
            }
            else
            {
                this.Status = DownloadStatus.Error;
                UpdateDownloadDisplay();
            }
        }

        // DownloadProgressChanged event handler
        public void DownloadProgressChangedHandler(object sender, EventArgs e)
        {
            // Update the UI every second
            if (DateTime.UtcNow > this.LastUpdateTime.AddSeconds(1))
            {
                CalculateDownloadSpeed();
                CalculateAverageRate();
                UpdateDownloadDisplay();
                this.LastUpdateTime = DateTime.UtcNow;
            }
        }
        // Pause download
        public void Pause()
        {
            if (this.Status == DownloadStatus.Waiting || this.Status == DownloadStatus.Downloading)
            {
                this.Status = DownloadStatus.Pausing;
            }
            if (this.Status == DownloadStatus.Queued)
            {
                this.Status = DownloadStatus.Paused;
                RaisePropertyChanged("StatusString");
            }
        }

        // Restart download
        public void Restart()
        {
            if (this.HasError || this.Status == DownloadStatus.Completed)
            {
                if (File.Exists(this.TempDownloadPath))
                {
                    File.Delete(this.TempDownloadPath);
                }
                if (File.Exists(this.DownloadPath))
                {
                    File.Delete(this.DownloadPath);
                }

                ResetProperties();
                this.Status = DownloadStatus.Waiting;
                UpdateDownloadDisplay();

                if (_downloadManager.ActiveDownloads > _userSettings.DownloadManager.MaxDownloads)
                {
                    this.Status = DownloadStatus.Queued;
                    RaisePropertyChanged("StatusString");
                    return;
                }

                DownloadThread = new Thread(new ThreadStart(DownloadFile));
                DownloadThread.IsBackground = true;
                DownloadThread.Start();
            }
        }

        // Start or continue download
        public void Start()
        {
            if (this.Status == DownloadStatus.Initialized || this.Status == DownloadStatus.Paused
                || this.Status == DownloadStatus.Queued || this.HasError)
            {
                if (!this.SupportsRange && this.DownloadedSize > 0)
                {
                    this.StatusString = "Error: Server does not support resume";
                    this.HasError = true;
                    this.RaiseDownloadCompleted();
                    return;
                }

                this.HasError = false;
                this.Status = DownloadStatus.Waiting;
                RaisePropertyChanged("StatusString");

                if (_downloadManager.ActiveDownloads > _userSettings.DownloadManager.MaxDownloads)
                {
                    this.Status = DownloadStatus.Queued;
                    RaisePropertyChanged("StatusString");
                    return;
                }

                // Start the download thread
                DownloadThread = new Thread(new ThreadStart(DownloadFile));
                DownloadThread.IsBackground = true;
                DownloadThread.Start();
            }
        }
    }
}
