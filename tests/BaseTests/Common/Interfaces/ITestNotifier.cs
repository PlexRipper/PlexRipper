using System;
using System.IO;

namespace PlexRipper.BaseTests
{
    public interface ITestNotifier
    {
        IObservable<Stream> CreatedDownloadStreams { get; }

        IObservable<Stream> CreatedFileMergeStreams { get; }

        void AddDownloadFileStream(Stream stream);

        void AddFileMergeStream(Stream stream);
    }
}