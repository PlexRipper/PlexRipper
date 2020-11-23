using System;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface IFileManager : ISetup
    {
        IObservable<FileMergeProgress> FileMergeProgressObservable { get; }

        /// <summary>
        /// Creates an FileTask from a completed <see cref="DownloadTask"/> and adds this to the database.
        /// </summary>
        /// <param name="downloadTask">The <see cref="DownloadTask"/> to be added as a <see cref="FileTask"/>.</param>
        Task<Result> AddFileTask(DownloadTask downloadTask);
    }
}