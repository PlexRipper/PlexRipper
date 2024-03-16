using Riok.Mapperly.Abstractions;

namespace PlexRipper.WebAPI.Common.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class DownloadTaskGenericToDownloadProgressDTOMapper
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
}