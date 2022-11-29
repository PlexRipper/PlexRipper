namespace PlexRipper.Application;

public interface IFileMerger : ISetupAsync, IBusy
{
    IObservable<FileMergeProgress> FileMergeProgressObservable { get; }

    IObservable<FileMergeProgress> FileMergeCompletedObservable { get; }

    IObservable<DownloadFileTask> FileMergeStartObservable { get; }

    /// <summary>
    /// Creates an FileTask from a completed <see cref="DownloadTask"/> and adds this to the database.
    /// </summary>
    /// <param name="downloadTaskId"></param>
    Task<Result> AddFileTaskFromDownloadTask(int downloadTaskId);

    Task ExecuteFileTasks();
}