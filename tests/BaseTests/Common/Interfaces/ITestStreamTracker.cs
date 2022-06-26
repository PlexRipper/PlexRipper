namespace PlexRipper.BaseTests
{
    public interface ITestStreamTracker
    {
        IObservable<Stream> CreatedDownloadStreams { get; }

        IObservable<Stream> CreatedFileMergeStreams { get; }

        void AddDownloadFileStream(Stream stream);

        void AddFileMergeStream(Stream stream);
    }
}