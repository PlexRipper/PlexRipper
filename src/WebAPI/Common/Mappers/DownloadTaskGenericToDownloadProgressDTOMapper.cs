using Riok.Mapperly.Abstractions;

namespace PlexRipper.WebAPI.Common.Mappers;

[Mapper]
public static partial class DownloadTaskGenericToDownloadProgressDTOMapper
{
    public static partial DownloadProgressDTO ToDownloadProgressDto(this DownloadTaskGeneric downloadTask);
}