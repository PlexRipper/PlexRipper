using PlexRipper.Domain;

namespace Data.Contracts;

public static class DownloadTaskExtensions
{
    public static IDownloadTaskProgress Calculate(this DownloadTaskGeneric downloadTask)
    {
        if (downloadTask.Children is null || !downloadTask.Children.Any())
            return downloadTask;

        downloadTask.DownloadSpeed = downloadTask.Children.Select(x => x.DownloadSpeed).Sum();
        downloadTask.FileTransferSpeed = downloadTask.Children.Select(x => x.FileTransferSpeed).Sum();
        downloadTask.DataReceived = downloadTask.Children.Select(x => x.DataReceived).Sum();
        downloadTask.DataTotal = downloadTask.Children.Select(x => x.DataTotal).Sum();
        downloadTask.Percentage = DataFormat.GetPercentage(downloadTask.DataReceived, downloadTask.DataTotal);
        downloadTask.DownloadStatus = DownloadTaskActions.Aggregate(downloadTask.Children.Select(x => x.DownloadStatus).ToList());

        return downloadTask;
    }

    public static IDownloadTaskProgress Calculate(this DownloadTaskMovie downloadTask)
    {
        if (downloadTask.Children is null || !downloadTask.Children.Any())
            return downloadTask;

        downloadTask.DownloadSpeed = downloadTask.Children.Select(x => x.DownloadSpeed).Sum();
        downloadTask.FileTransferSpeed = downloadTask.Children.Select(x => x.FileTransferSpeed).Sum();
        downloadTask.DataReceived = downloadTask.Children.Select(x => x.DataReceived).Sum();
        downloadTask.DataTotal = downloadTask.Children.Select(x => x.DataTotal).Sum();
        downloadTask.Percentage = DataFormat.GetPercentage(downloadTask.DataReceived, downloadTask.DataTotal);
        downloadTask.DownloadStatus = DownloadTaskActions.Aggregate(downloadTask.Children.Select(x => x.DownloadStatus).ToList());

        return downloadTask;
    }

    /// <summary>
    /// Calculate <see cref="IDownloadTaskProgress"/> properties such as DataReceived, DataTotal based on the nested children.
    /// </summary>
    public static IDownloadTaskProgress Calculate(this DownloadTaskTvShow downloadTask)
    {
        if (downloadTask.Children is null || !downloadTask.Children.Any())
            return downloadTask;

        downloadTask.DownloadSpeed = downloadTask.Children.Select(x => x.DownloadSpeed).Sum();
        downloadTask.FileTransferSpeed = downloadTask.Children.Select(x => x.FileTransferSpeed).Sum();
        downloadTask.DataReceived = downloadTask.Children.Select(x => x.DataReceived).Sum();
        downloadTask.DataTotal = downloadTask.Children.Select(x => x.DataTotal).Sum();
        downloadTask.Percentage = DataFormat.GetPercentage(downloadTask.DataReceived, downloadTask.DataTotal);
        downloadTask.DownloadStatus = DownloadTaskActions.Aggregate(downloadTask.Children.Select(x => x.DownloadStatus).ToList());

        return downloadTask;
    }

    /// <summary>
    /// Calculate <see cref="IDownloadTaskProgress"/> properties such as DataReceived, DataTotal based on the nested children.
    /// </summary>
    public static IDownloadTaskProgress Calculate(this DownloadTaskTvShowSeason downloadTask)
    {
        if (downloadTask.Children is null || !downloadTask.Children.Any())
            return downloadTask;

        downloadTask.DownloadSpeed = downloadTask.Children.Select(x => x.DownloadSpeed).Sum();
        downloadTask.FileTransferSpeed = downloadTask.Children.Select(x => x.FileTransferSpeed).Sum();
        downloadTask.DataReceived = downloadTask.Children.Select(x => x.DataReceived).Sum();
        downloadTask.DataTotal = downloadTask.Children.Select(x => x.DataTotal).Sum();
        downloadTask.Percentage = DataFormat.GetPercentage(downloadTask.DataReceived, downloadTask.DataTotal);
        downloadTask.DownloadStatus = DownloadTaskActions.Aggregate(downloadTask.Children.Select(x => x.DownloadStatus).ToList());

        return downloadTask;
    }

    /// <summary>
    /// Calculate <see cref="IDownloadTaskProgress"/> properties such as DataReceived, DataTotal based on the nested children.
    /// </summary>
    public static IDownloadTaskProgress Calculate(this DownloadTaskTvShowEpisode downloadTask)
    {
        if (downloadTask.Children is null || !downloadTask.Children.Any())
            return downloadTask;

        downloadTask.DownloadSpeed = downloadTask.Children.Select(x => x.DownloadSpeed).Sum();
        downloadTask.FileTransferSpeed = downloadTask.Children.Select(x => x.FileTransferSpeed).Sum();
        downloadTask.DataReceived = downloadTask.Children.Select(x => x.DataReceived).Sum();
        downloadTask.DataTotal = downloadTask.Children.Select(x => x.DataTotal).Sum();
        downloadTask.Percentage = DataFormat.GetPercentage(downloadTask.DataReceived, downloadTask.DataTotal);
        downloadTask.DownloadStatus = DownloadTaskActions.Aggregate(downloadTask.Children.Select(x => x.DownloadStatus).ToList());

        return downloadTask;
    }
}