using LukeHagar.PlexAPI.SDK.Models.Requests;

namespace PlexRipper.PlexApi;

public static class PlexApiPartMapper
{
    public static PlexMediaDataPart ToPlexMediaDataPart(this GetLibraryItemsPart source) =>
        new()
        {
            Duration = source.Duration,
            File = source.File.GetFileName(),
            Size = source.Size,
            AudioProfile = source.AudioProfile ?? string.Empty,
            Container = source.Container,
            Indexes = source.Indexes ?? string.Empty,
            VideoProfile = source.VideoProfile,
            ObfuscatedFilePath = source.Key,
        };

    public static List<PlexMediaDataPart> ToPlexMediaDataPart(this List<GetLibraryItemsPart> source)
    {
        return source.Select(x => x.ToPlexMediaDataPart()).ToList();
    }
}
