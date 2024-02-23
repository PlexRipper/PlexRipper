using FluentResults;

namespace DownloadManager.Contracts;

public interface IDownloadFileStream
{
    Result<Stream> CreateDownloadFileStream(string directory, string fileName, long fileSize);
}