using PlexRipper.Domain;
using Riok.Mapperly.Abstractions;

namespace Application.Contracts;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class DownloadTaskGenericToDTOMapper
{
    public static DownloadProgressDTO ToDownloadProgressDto(this DownloadTaskGeneric downloadTask)
    {
        if (downloadTask is null)
            return null;

        var result = downloadTask.ToDownloadProgressDtoMapper();
        result.Actions = DownloadTaskActions.Convert(result.Status);
        return result;
    }

    [MapProperty(nameof(DownloadTaskGeneric.DownloadStatus), nameof(DownloadProgressDTO.Status))]
    private static partial DownloadProgressDTO ToDownloadProgressDtoMapper(this DownloadTaskGeneric downloadTask);

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