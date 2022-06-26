﻿using System.Reactive.Subjects;
using FluentResults;

namespace PlexRipper.FileSystem.Common
{
    public interface IFileMergeStreamProvider
    {
        Task<Result<Stream>> CreateMergeStream(string destinationDirectory);

        Task MergeFiles(List<string> filePaths, Stream destination, Subject<long> bytesReceivedProgress,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}