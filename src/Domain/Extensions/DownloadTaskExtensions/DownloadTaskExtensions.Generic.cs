namespace PlexRipper.Domain;

public static partial class DownloadTaskExtensions
{
    public static List<DownloadWorkerTask> GenerateDownloadWorkerTasks(
        this DownloadTaskFileBase downloadTask,
        int parts
    )
    {
        // Create download worker tasks/segments/ranges
        var totalBytesToReceive = downloadTask.DataTotal;
        var partSize = totalBytesToReceive / parts;
        var remainder = totalBytesToReceive - partSize * parts;

        var downloadWorkerTasks = new List<DownloadWorkerTask>();

        for (var i = 0; i < parts; i++)
        {
            var startPosition = partSize * i;
            var endPosition = startPosition + partSize;
            if (i == parts - 1 && remainder > 0)
            {
                // Add the remainder to the last download range
                endPosition += remainder;
            }

            var partIndex = i + 1;

            downloadWorkerTasks.Add(
                new DownloadWorkerTask
                {
                    DownloadTaskId = downloadTask.Id,
                    PlexServerId = downloadTask.PlexServerId,
                    DownloadDirectory = downloadTask.DownloadDirectory,
                    FileLocationUrl = downloadTask.FileLocationUrl,
                    PartIndex = partIndex,
                    StartByte = startPosition,
                    EndByte = endPosition,
                    FileName = downloadTask.FileName.AddPartIndexToFileName(partIndex),
                    DownloadStatus = DownloadStatus.Queued,
                    BytesReceived = 0,
                    ElapsedTime = 0,
                }
            );
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
                downloadTask.DownloadWorkerTasks.Select(x => x.DownloadFilePath).ToArray()
            ),
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
