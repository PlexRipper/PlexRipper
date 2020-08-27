using System;
using System.IO;

namespace PlexRipper.DownloadManager.Common
{
    public class DownloadRange
    {
        private readonly string _downloadDirectory;
        private readonly string _fileName;

        public DownloadRange(int id, string url, long startByte, long endByte, string fileName, string downloadDirectory)
        {
            _fileName = fileName;
            _downloadDirectory = downloadDirectory;
            Id = id;
            Url = url;
            StartByte = startByte;
            EndByte = endByte;
        }

        public int Id { get; }
        public string Url { get; }
        public Uri Uri => new Uri(Url);

        public long StartByte { get; }
        public long EndByte { get; }
        public long RangeSize => EndByte - StartByte;

        #region Directories

        /// <summary>
        /// The download directory with a folder named after the filename
        /// </summary>
        public string TempDirectory => Path.Combine(_downloadDirectory, $"{Path.GetFileNameWithoutExtension(_fileName)}");

        public string TempFileName => $"{Id}-{_fileName}";

        public string TempFilePath => Path.Combine(TempDirectory, TempFileName);
        #endregion


        public long BytesRemaining => RangeSize - BytesReceived;

        /// <summary>
        /// Gets the total bytes received for download
        /// </summary>
        public long BytesReceived { get; set; }
    }
}