using System;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Types.FileSystem;

namespace PlexRipper.Application.Common
{
    public interface IFileManager
    {
        void AddFileTask(FileTask fileTask);
        IObservable<FileMergeProgress> FileMergeProgressObservable { get; }

        /// <summary>
        /// Creates an FileTask from a completed <see cref="DownloadTask"/> and adds this to the database.
        /// </summary>
        /// <param name="downloadTask">The <see cref="DownloadTask"/> to be added as a <see cref="FileTask"/>.</param>
        void AddFileTask(DownloadTask downloadTask);
    }
}