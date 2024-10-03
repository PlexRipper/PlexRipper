using LukeHagar.PlexAPI.SDK.Models.Requests;

namespace PlexRipper.PlexApi;

public static class PlexMediumMapper
{
    public static PlexMediaData ToPlexMediaData(this GetLibraryItemsMedia source) =>
        new()
        {
            Duration = source.Duration ?? 0,
            Bitrate = source.Bitrate ?? 0,
            Width = source.Width ?? 0,
            Height = source.Height ?? 0,
            AspectRatio = source.AspectRatio ?? 0,
            AudioChannels = source.AudioChannels ?? 0,
            AudioCodec = source.AudioCodec ?? string.Empty,
            VideoCodec = source.VideoCodec ?? string.Empty,
            VideoResolution = source.VideoResolution ?? string.Empty,
            VideoFrameRate = source.VideoFrameRate ?? string.Empty,
            AudioProfile = source.AudioProfile ?? string.Empty,
            VideoProfile = source.VideoProfile ?? string.Empty,
            Parts = source.Part.ToList().ToPlexMediaDataPart(),
            MediaFormat = source.Container,
        };

    public static List<PlexMediaData> ToPlexMediaData(this List<GetLibraryItemsMedia> source)
    {
        return source.Select(x => x.ToPlexMediaData()).ToList();
    }
}
