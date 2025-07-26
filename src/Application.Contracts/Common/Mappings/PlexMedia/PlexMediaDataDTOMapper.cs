using PlexRipper.Domain;

namespace Application.Contracts;

public static class PlexMediaDataDTOMapper
{
    #region MediaData

    public static PlexMediaDataDTO ToDTO(this LibraryMediaItemMediaDTO source) =>
        new()
        {
            Duration = source.Duration,
            VideoResolution = source.VideoResolution,
            Width = source.Width,
            Height = source.Height,
            Bitrate = source.Bitrate,
            VideoCodec = source.VideoCodec,
            VideoFrameRate = source.VideoFrameRate,
            AspectRatio = source.AspectRatio,
            VideoProfile = source.VideoProfile,
            AudioProfile = source.AudioProfile,
            AudioCodec = source.AudioCodec,
            AudioChannels = source.AudioChannels,
            Parts = source.Parts,
        };

    public static List<PlexMediaDataDTO> ToDTO(this List<LibraryMediaItemMediaDTO> source) => source.ConvertAll(ToDTO);

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
            VideoProfile = source.VideoProfile,
        };

    public static List<PlexMediaDataPartDTO> ToDTO(this List<LibraryMediaItemPartDTO> source) =>
        source.ConvertAll(ToDTO);

    #endregion
}
