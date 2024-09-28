using PlexRipper.Domain;

namespace Application.Contracts;

public static class PlexMediaDataDTOMapper
{
    #region MediaData

    public static PlexMediaDataDTO ToDTO(this PlexMediaData source) =>
        new()
        {
            MediaFormat = source.MediaFormat,
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
            Parts = source.Parts.ConvertAll(ToDTO),
        };

    public static List<PlexMediaDataDTO> ToDTO(this List<PlexMediaData> source) => source.ConvertAll(ToDTO);

    #endregion

    #region PlexMediaQuality

    public static PlexMediaQualityDTO ToDTO(this PlexMediaQuality source) =>
        new()
        {
            Quality = source.Quality,
            DisplayQuality = source.DisplayQuality,
            HashId = source.HashId,
        };

    public static List<PlexMediaQualityDTO> ToDTO(this List<PlexMediaQuality> source) => source.ConvertAll(ToDTO);

    #endregion

    #region PlexMediaDataPart

    public static PlexMediaDataPartDTO ToDTO(this PlexMediaDataPart source) =>
        new()
        {
            ObfuscatedFilePath = source.ObfuscatedFilePath,
            Duration = source.Duration,
            File = source.File.GetFileName(),
            Size = source.Size,
            Container = source.Container,
            VideoProfile = source.VideoProfile,
        };

    public static List<PlexMediaDataPartDTO> ToDTO(this List<PlexMediaDataPart> source) => source.ConvertAll(ToDTO);

    #endregion
}
