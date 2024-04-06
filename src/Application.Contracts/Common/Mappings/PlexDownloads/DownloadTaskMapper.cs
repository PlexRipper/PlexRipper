using PlexRipper.Domain;

namespace Application.Contracts;

public static partial class DownloadTaskGenericToDTOMapper
{
    #region ToDTO

    public static DownloadTaskDTO ToDTO(this DownloadTaskGeneric downloadTask, string DownloadUrl = "") => new()
    {
        Id = downloadTask.Id,
        Key = downloadTask.MediaKey,
        Title = downloadTask.Title,
        FullTitle = downloadTask.FullTitle,
        MediaType = downloadTask.MediaType,
        DownloadTaskType = downloadTask.DownloadTaskType,
        Status = downloadTask.DownloadStatus,
        Percentage = downloadTask.Percentage,
        DataReceived = downloadTask.DataReceived,
        DataTotal = downloadTask.DataTotal,
        CreatedAt = downloadTask.CreatedAt,
        FileName = downloadTask.FileName,
        TimeRemaining = downloadTask.TimeRemaining,
        DownloadDirectory = downloadTask.DownloadDirectory,
        DestinationDirectory = downloadTask.DestinationDirectory,
        FileLocationUrl = downloadTask.FileLocationUrl,
        DownloadSpeed = downloadTask.DownloadSpeed,
        FileTransferSpeed = downloadTask.FileTransferSpeed,
        DownloadUrl = DownloadUrl,
        PlexServerId = downloadTask.PlexServerId,
        PlexLibraryId = downloadTask.PlexLibraryId,
        ParentId = downloadTask.ParentId,
        Children = downloadTask.Children.Select(x => x.ToDTO(DownloadUrl)).ToList(),
        Actions = DownloadTaskActions.Convert(downloadTask.DownloadStatus),
    };

    #endregion

    #region DownloadProgress

    public static List<ServerDownloadProgressDTO> ToServerDownloadProgressDTOList(this List<DownloadTaskGeneric> source)
    {
        var serverDownloads = new List<ServerDownloadProgressDTO>();
        foreach (var serverId in source.Select(x => x.PlexServerId).Distinct())
        {
            var downloadTasks = source.Where(x => x.PlexServerId == serverId).ToList();
            var downloads = downloadTasks.ToDownloadProgressDto();
            serverDownloads.Add(new ServerDownloadProgressDTO
            {
                Id = serverId,
                DownloadableTasksCount = downloadTasks.Flatten(x => x.Children).ToList().FindAll(x => x.IsDownloadable).Count,
                Downloads = downloads,
            });
        }

        return serverDownloads;
    }

    public static List<DownloadProgressDTO> ToDownloadProgressDto(this List<DownloadTaskGeneric>? downloadTasks)
    {
        var result = new List<DownloadProgressDTO>();
        if (downloadTasks is null || !downloadTasks.Any())
            return new List<DownloadProgressDTO>();

        foreach (var downloadTask in downloadTasks)
            result.Add(new DownloadProgressDTO
            {
                Id = downloadTask.Id,
                Title = downloadTask.Title,
                MediaType = downloadTask.MediaType,
                Status = downloadTask.DownloadStatus,
                Percentage = downloadTask.Percentage,
                DataReceived = downloadTask.DataReceived,
                DataTotal = downloadTask.DataTotal,
                TimeRemaining = downloadTask.TimeRemaining,
                DownloadSpeed = downloadTask.DownloadSpeed,
                FileTransferSpeed = downloadTask.FileTransferSpeed,
                Children = downloadTask.Children.ToDownloadProgressDto(),
                Actions = DownloadTaskActions.Convert(downloadTask.DownloadStatus),
            });

        return result;
    }

    #endregion
}