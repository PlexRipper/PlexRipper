namespace PlexRipper.Application;

public interface IDownloadTaskFactory
{
    Task<Result<List<DownloadTask>>> GenerateAsync(List<DownloadMediaDTO> downloadMedias);

    /// <summary>
    /// Creates <see cref="DownloadTask"/>s from a <see cref="PlexMovie"/> and send it to the <see cref="IDownloadManager"/>.
    /// </summary>
    /// <param name="plexMovieIds">The ids of the <see cref="PlexMovie"/> to create <see cref="DownloadTask"/>s from.</param>
    /// <returns>The created <see cref="DownloadTask"/>.</returns>
    Task<Result<List<DownloadTask>>> GenerateMovieDownloadTasksAsync(List<int> plexMovieIds);

    /// <summary>
    /// Regenerates <see cref="DownloadTask">DownloadTasks</see> while maintaining the Id and priority.
    /// Will also remove old <see cref="DownloadWorkerTask">DownloadWorkerTasks</see> assigned to the old downloadTasks from the database.
    /// </summary>
    /// <param name="downloadTaskIds"></param>
    /// <returns>A list of newly generated <see cref="DownloadTask">DownloadTasks</see></returns>
    Task<Result<List<DownloadTask>>> RegenerateDownloadTask(List<int> downloadTaskIds);

    Result<List<DownloadWorkerTask>> GenerateDownloadWorkerTasks(DownloadTask downloadTask);
}