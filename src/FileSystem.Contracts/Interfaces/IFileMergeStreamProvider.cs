using System.Reactive.Subjects;
using FluentResults;
using PlexRipper.Domain;

namespace FileSystem.Contracts;

public interface IFileMergeStreamProvider
{
    Task<Result<Stream>> OpenOrCreateMergeStream(string fileDestinationPath);

    Task MergeFiles(
        FileTask fileTask,
        Stream destination,
        Subject<long> bytesReceivedProgress,
        CancellationToken cancellationToken = default
    );
}
