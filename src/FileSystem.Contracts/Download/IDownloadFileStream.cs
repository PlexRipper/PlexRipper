using FluentResults;

namespace FileSystem.Contracts;

public interface IDownloadFileStream
{
    Result<Stream> CreateDownloadFileStream(string directory, string fileName, long fileSize);
}