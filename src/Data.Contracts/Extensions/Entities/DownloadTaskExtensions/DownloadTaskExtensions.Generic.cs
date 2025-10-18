using Reaparr.Domain;

namespace Reaparr.Data.Contracts;

public static partial class DownloadTaskExtensions
{
    private const string TempDownloadFileSuffix = ".reaptemp";

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
                    FileName = downloadTask.FileName.AddReaparrTempSuffixToFileName(),
                    DownloadStatus = DownloadStatus.Queued,
                    BytesReceived = 0,
                    ElapsedTime = 0,
                }
            );
        }

        return downloadWorkerTasks;
    }

    private static string AddReaparrTempSuffixToFileName(this string fileName) =>
        $"{Path.GetFileNameWithoutExtension(fileName)}{Path.GetExtension(fileName)}{TempDownloadFileSuffix}";

    public static string RemoveReapTempSuffix(this string filePath) =>
        filePath.EndsWith(TempDownloadFileSuffix, StringComparison.OrdinalIgnoreCase)
            ? filePath[..^TempDownloadFileSuffix.Length]
            : filePath;
}
