using PlexRipper.Domain;
using Riok.Mapperly.Abstractions;

namespace Application.Contracts;

[Mapper]
public static partial class DownloadTaskGenericToDTOMapper
{
    #region ToDTO

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(DownloadTaskGeneric.DownloadStatus), nameof(DownloadTaskDTO.Status))]
    [MapperIgnoreTarget(nameof(DownloadTaskDTO.Quality))]
    [MapperIgnoreTarget(nameof(DownloadTaskDTO.DownloadUrl))]
    [MapperIgnoreTarget(nameof(DownloadTaskDTO.Actions))]
    public static partial DownloadTaskDTO ToDTO(this DownloadTaskGeneric downloadTask);

    #endregion

    #region DownloadProgress

    public static DownloadProgressDTO ToDownloadProgressDto(this DownloadTaskGeneric downloadTask)
    {
        if (downloadTask is null)
            return null;

        var result = downloadTask.ToDownloadProgressDtoMapper();
        result.Actions = DownloadTaskActions.Convert(result.Status);
        return result;
    }

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(DownloadTaskGeneric.DownloadStatus), nameof(DownloadProgressDTO.Status))]
    [MapperIgnoreTarget(nameof(DownloadProgressDTO.Actions))]
    [MapperIgnoreSource(nameof(DownloadTaskGeneric.Key))]
    [MapperIgnoreSource(nameof(DownloadTaskGeneric.FullTitle))]
    [MapperIgnoreSource(nameof(DownloadTaskGeneric.DownloadTaskType))]
    [MapperIgnoreSource(nameof(DownloadTaskGeneric.CreatedAt))]
    [MapperIgnoreSource(nameof(DownloadTaskGeneric.FileName))]
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

    #endregion
}