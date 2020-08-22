﻿using System;

 namespace PlexRipper.DownloadManager.Common
{
    public class DownloadRange
    {
        public int Id { get; set; }
        public long StartByte { get; set; }
        public long EndByte { get; set; }
        public long CurrentOffset { get; set; }
        public bool IsDone { get; set; }
        public string TempFileName  {get;}
        public long Size => EndByte - StartByte;
        public DownloadRange(long startByte, long endByte)
        {
            TempFileName = Guid.NewGuid().ToString();
            StartByte = startByte;
            EndByte = endByte;
        }
    }
}
