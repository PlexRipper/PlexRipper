using System;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface IFileMerger : ISetupAsync
    {
        IObservable<FileMergeProgress> FileMergeProgressObservable { get; }

        /// <summary>
        /// Creates an FileTask from a completed <see cref="DownloadTask"/> and adds this to the database.
        /// </summary>
        /// <param name="downloadTaskId"></param>
        Task<Result> AddFileTaskFromDownloadTask(int downloadTaskId);

        Task ExecuteFileTasks();
    }
}