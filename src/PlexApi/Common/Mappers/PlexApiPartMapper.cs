using PlexRipper.PlexApi.Models;

namespace PlexRipper.PlexApi;

public static class PlexApiPartMapper
{
    public static PlexMediaDataPart ToPlexMediaDataPart(this Part source)
    {
        return new PlexMediaDataPart
        {
            Duration = source.Duration,
            File = source.File,
            Size = source.Size,
            AudioProfile = source.AudioProfile,
            Container = source.Container,
            HasThumbnail = source.HasThumbnail,
            Indexes = source.Indexes,
            HasChapterTextStream = source.HasChapterTextStream,
            VideoProfile = source.VideoProfile,
            ObfuscatedFilePath = source.Key,
        };
    }

    public static List<PlexMediaDataPart> ToPlexMediaDataPart(this List<Part> source)
    {
        return source.Select(x => x.ToPlexMediaDataPart()).ToList();
    }
}
