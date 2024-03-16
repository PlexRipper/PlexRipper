namespace PlexRipper.Domain;

public static partial class DownloadTaskExtensions
{
    public static List<DownloadWorkerTask> GenerateDownloadWorkerTasks(this DownloadTaskGeneric downloadTask, int parts)
    {
        // Create download worker tasks/segments/ranges
        var totalBytesToReceive = downloadTask.DataTotal;
        var partSize = totalBytesToReceive / parts;
        var remainder = totalBytesToReceive - partSize * parts;

        var downloadWorkerTasks = new List<DownloadWorkerTask>();

        for (var i = 0; i < parts; i++)
        {
            var start = partSize * i;
            var end = start + partSize;
            if (i == parts - 1 && remainder > 0)
            {
                // Add the remainder to the last download range
                end += remainder;
            }

            downloadWorkerTasks.Add(new DownloadWorkerTask(downloadTask, i + 1, start, end));
        }

        return downloadWorkerTasks;
    }

    public static FileTask ToFileTask(this DownloadTaskGeneric downloadTask)
    {
        if (!downloadTask.DownloadWorkerTasks.Any())
            throw new Exception("DownloadWorkerTasks must be included before converting to FileTask");

        return new FileTask
        {
            Id = 0,
            CreatedAt = DateTime.UtcNow,
            FilePathsCompressed = string.Join(
                ';',
                downloadTask.DownloadWorkerTasks.Select(x => x.TempFilePath)
                    .ToArray()),
            FileName = downloadTask.FileName,
            FileSize = downloadTask.DataTotal,
            DownloadTaskId = downloadTask.Id,
            DownloadTaskType = downloadTask.DownloadTaskType,
            PlexServer = null,
            PlexServerId = downloadTask.PlexServerId,
            PlexLibrary = null,
            PlexLibraryId = downloadTask.PlexLibraryId,
            DestinationDirectory = downloadTask.DestinationDirectory,
        };
    }
}