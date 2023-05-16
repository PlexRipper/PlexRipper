using PlexRipper.PlexApi.Models;

namespace PlexRipper.PlexApi;

public static class PlexMediumMapper
{
    public static PlexMediaData ToPlexMediaData(this Medium source)
    {
        return new PlexMediaData
        {
            Duration = source.Duration,
            Bitrate = source.Bitrate,
            Width = source.Width,
            Height = source.Height,
            AspectRatio = source.AspectRatio,
            AudioChannels = source.AudioChannels,
            AudioCodec = source.AudioCodec,
            VideoCodec = source.VideoCodec,
            VideoResolution = source.VideoResolution,
            VideoFrameRate = source.VideoFrameRate,
            AudioProfile = source.AudioProfile,
            VideoProfile = source.VideoProfile,
            Protocol = source.Protocol,
            OptimizedForStreaming = source.OptimizedForStreaming == 1,
            Selected = source.Selected,
            Parts = source.Part.ToList().ToPlexMediaDataPart(),
            MediaFormat = source.Container,
        };
    }

    public static List<PlexMediaData> ToPlexMediaData(this List<Medium> source)
    {
        return source.Select(x => x.ToPlexMediaData()).ToList();
    }
}