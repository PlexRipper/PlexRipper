﻿using System;

 namespace PlexRipper.DownloadManager.Common
{
    public class DownloadRange
    {
        public int Id { get; set; }
        public string Url { get; }
        public Uri Uri => new Uri(Url);

        public long StartByte { get; set; }
        public long EndByte { get; set; }
        public long CurrentOffset { get; set; }
        public bool IsDone { get; set; }
        public string TempFileName  {get;}
        public long RangeSize => EndByte - StartByte;

        public long BytesRemaining => RangeSize - BytesReceived;
        /// <summary>
        /// Gets the total bytes received for download
        /// </summary>
        public long BytesReceived { get; set; }
        public DownloadRange(string url, long startByte, long endByte)
        {
            TempFileName = Guid.NewGuid().ToString();
            Url = url;
            StartByte = startByte;
            EndByte = endByte;
        }
    }
}
