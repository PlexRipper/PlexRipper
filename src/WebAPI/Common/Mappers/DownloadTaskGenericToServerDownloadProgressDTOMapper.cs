using PlexRipper.WebAPI.SignalR.Common;

namespace PlexRipper.WebAPI.Common.Mappers;

public static class DownloadTaskGenericToServerDownloadProgressDTOMapper
{
    public static List<ServerDownloadProgressDTO> ToServerDownloadProgressDTOList(this List<DownloadTaskGeneric> source)
    {
        var serverDownloads = new List<ServerDownloadProgressDTO>();
        foreach (var serverId in source.Select(x => x.PlexServerId).Distinct())
        {
            var downloadTasks = source.Where(x => x.PlexServerId == serverId).ToList();
            var downloads = downloadTasks.Select(x => x.ToDownloadProgressDto()).ToList();
            serverDownloads.Add(new ServerDownloadProgressDTO
            {
                Id = serverId,
                DownloadableTasksCount = downloadTasks.Flatten(x => x.Children).ToList().FindAll(x => x.IsDownloadable).Count,
                Downloads = downloads,
            });
        }

        return serverDownloads;
    }
}