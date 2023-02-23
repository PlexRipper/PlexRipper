using AutoMapper;
using PlexRipper.DownloadManager;
using PlexRipper.WebAPI.SignalR.Common;

namespace PlexRipper.WebAPI;

public class ListDownloadTaskToListServerDownloadProgressDTOConverter : ITypeConverter<List<DownloadTask>, List<ServerDownloadProgressDTO>>
{
    public List<ServerDownloadProgressDTO> Convert(List<DownloadTask> source, List<ServerDownloadProgressDTO> destination, ResolutionContext context)
    {
        var serverDownloads = new List<ServerDownloadProgressDTO>();
        foreach (var serverId in source.Select(x => x.PlexServerId).Distinct())
        {
            var downloadTasks = source.Where(x => x.PlexServerId == serverId).ToList();
            var downloadTaskDTO = context.Mapper.Map<List<DownloadProgressDTO>>(downloadTasks);
            serverDownloads.Add(new ServerDownloadProgressDTO
            {
                Id = serverId,
                Downloads = downloadTaskDTO,
            });
        }

        return serverDownloads;
    }
}