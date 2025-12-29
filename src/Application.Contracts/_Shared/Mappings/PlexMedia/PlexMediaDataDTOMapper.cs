using Reaparr.Domain;

namespace Reaparr.Application.Contracts;

public static class PlexMediaDataDTOMapper
{
    #region MediaData

    public static PlexMediaDataDTO ToDTO(this LibraryMediaItemMediaDTO source) =>
        new()
        {
            Duration = source.Duration,
            VideoResolution = source.VideoResolution,
            VideoCodec = source.VideoCodec,
            AudioCodec = source.AudioCodec,
        };

    #endregion

    #region PlexMediaDataPart

    public static PlexMediaDataPartDTO ToDTO(this LibraryMediaItemPartDTO source) =>
        new()
        {
            ObfuscatedFilePath = source.Key,
            Duration = source.Duration,
            File = source.File.GetFileName(),
            Size = source.Size,
            Container = source.Container,
        };

    #endregion
}
