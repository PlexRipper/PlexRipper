namespace PlexRipper.DownloadManager;

public interface IDownloadFileStream
{
    Result<Stream> CreateDownloadFileStream(string directory, string fileName, long fileSize);
}