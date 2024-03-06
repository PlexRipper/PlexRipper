namespace PlexRipper.Domain;

public static class DownloadTaskGenericExtensions
{
    public static void GenerateDownloadWorkerTasks(this DownloadTaskGeneric downloadTask, int parts)
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

        downloadTask.DownloadWorkerTasks = downloadWorkerTasks;
    }
}