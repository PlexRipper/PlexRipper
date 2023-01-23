namespace PlexRipper.WebAPI.Common.DTO.PlexMediaData;

public class PlexMediaDataDTO
{
    #region Properties

    public string MediaFormat { get; set; }

    public long Duration { get; set; }

    public string VideoResolution { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public int Bitrate { get; set; }

    public string VideoCodec { get; set; }

    public string VideoFrameRate { get; set; }

    public double AspectRatio { get; set; }

    public string VideoProfile { get; set; }

    public string AudioProfile { get; set; }

    public string AudioCodec { get; set; }

    public int AudioChannels { get; set; }

    public List<PlexMediaDataPartDTO> Parts { get; set; }

    #endregion
}