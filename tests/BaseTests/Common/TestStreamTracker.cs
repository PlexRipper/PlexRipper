using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace PlexRipper.BaseTests
{
    public class TestStreamTracker : ITestStreamTracker
    {
        #region Fields

        private readonly Subject<Stream> _downloadFileMergeStream = new();

        private readonly Subject<Stream> _downloadFileStreams = new();

        #endregion

        #region Properties

        public IObservable<Stream> CreatedDownloadStreams => _downloadFileStreams.AsObservable();

        public IObservable<Stream> CreatedFileMergeStreams => _downloadFileMergeStream.AsObservable();

        #endregion

        #region Public Methods

        public void AddDownloadFileStream(Stream stream)
        {
            _downloadFileStreams.OnNext(stream);
        }

        public void AddFileMergeStream(Stream stream)
        {
            _downloadFileMergeStream.OnNext(stream);
        }

        #endregion
    }
}