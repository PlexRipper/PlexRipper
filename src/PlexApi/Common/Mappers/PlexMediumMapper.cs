using LukeHagar.PlexAPI.SDK.Models.Requests;

namespace PlexRipper.PlexApi;

public static class PlexMediumMapper
{
    public static PlexMediaData ToPlexMediaData(this GetLibraryItemsMedia source) =>
        new()
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
            AudioProfile = source.AudioProfile ?? string.Empty,
            VideoProfile = source.VideoProfile,
            Parts = source.Part.ToList().ToPlexMediaDataPart(),
            MediaFormat = source.Container,
        };

    public static List<PlexMediaData> ToPlexMediaData(this List<GetLibraryItemsMedia> source)
    {
        return source.Select(x => x.ToPlexMediaData()).ToList();
    }
}
