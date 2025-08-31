using Reaparr.Domain;

namespace Reaparr.Data.Contracts;

public static partial class DownloadTaskExtensions
{
    public static List<DownloadWorkerTask> GenerateDownloadWorkerTasks(
        this DownloadTaskFileBase downloadTask,
        int parts
    )
    {
        if (parts <= 0)
            return [];

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
}
