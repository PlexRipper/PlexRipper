using System.Text.RegularExpressions;

namespace Reaparr.PlexApi;

public static partial class PlexMediaDataMapper
{
    [GeneratedRegex("[^0-9]")]
    private static partial Regex MyRegex();

    private static string? GetImdbId(this List<MetaDataGuidsDTO> guids) =>
        guids.Find(x => x.Id.Contains("imdb"))?.Id.Replace("imdb://", "");

    private static int? GetTmdbId(this List<MetaDataGuidsDTO> guids)
    {
        var id = guids.Find(x => x.Id.Contains("tmdb"))?.Id.Replace("tmdb://", "");
        return int.TryParse(id, out var result) ? result : null;
    }

    private static int? GetTvdbId(this List<MetaDataGuidsDTO> guids)
    {
        var id = guids.Find(x => x.Id.Contains("tvdb"))?.Id.Replace("tvdb://", "");
        return int.TryParse(id, out var result) ? result : null;
    }

    /// <summary>
    /// Retrieves the MetaDataKey from either the ThumbUrl,BannerUrl, ArtUrl or ThemeUrl.
    /// It is assumed that all MetaDataKeys are the same, returns 0 if nothing is found.
    /// </summary>
    /// <param name="metadata"></param>
    /// <returns></returns>
    private static int RetrieveMetaDataKey(LibraryMediaItemDTO metadata)
    {
        List<string> list = [metadata.Thumb, metadata.Art, metadata.Theme];

        foreach (var entry in list)
            if (!string.IsNullOrEmpty(entry))
            {
                // We want the last number
                // Example: /library/metadata/457047/thumb/1587006741
                var splitStrings = entry.Split('/').ToList();
                if (splitStrings.Count > 2)
                {
                    if (int.TryParse(splitStrings.Last(), out var result))
                        return result;
                }
            }

        return 0;
    }

    /// <summary>
    /// The PlexAPI is sometimes missing the ParentKey, this method will attempt to get the ParentKey from the ParentGuid.
    /// </summary>
    /// <param name="originalSource"> The original source to get the ParentKey from.</param>
    /// <returns>The ParentKey if found and otherwise -1.</returns>
    private static int GetParentKey(this LibraryMediaItemDTO originalSource)
    {
        var parentKeyString = !string.IsNullOrEmpty(originalSource.ParentRatingKey)
            ? originalSource.ParentRatingKey
            : "-1";

        if (int.TryParse(parentKeyString, out var parentKey))
        {
            return parentKey;
        }

        var parentGuid = !string.IsNullOrEmpty(originalSource.ParentGuid) ? originalSource.ParentGuid : string.Empty;
        if (string.IsNullOrEmpty(parentGuid))
        {
            return -1;
        }

        if (parentGuid.Contains("local"))
        {
            // Replace all non-numeric characters
            var result = MyRegex().Replace(originalSource.ParentGuid, "");
            if (!int.TryParse(result, out parentKey))
            {
                parentKey = -1;
            }
        }

        return parentKey;
    }

    private static ReleaseSource DetermineReleaseSource(this LibraryMediaItemMediaDTO mediaData)
    {
        var container = mediaData.Container.ToLowerInvariant();
        var audioCodec = mediaData.AudioCodec.ToLowerInvariant();
        var audioProfile = mediaData.AudioProfile.ToLowerInvariant();

        // DVD
        if (container is "vob" or "mpg" or "mpeg")
            return ReleaseSource.DVD;

        // Blu-ray (container-level)
        if (container is "m2ts" or "bdmv")
        {
            var isLosslessAudio =
                audioCodec is "truehd" or "dts-hd ma" or "dts-hd.ma"
                || (audioCodec == "dca" && audioProfile.Contains("ma"));

            return isLosslessAudio ? ReleaseSource.BluRayRemux : ReleaseSource.BluRay;
        }

        // HDTV / broadcast
        if (container is "ts" or "mpegts")
            return ReleaseSource.HDTV;

        // Web-based containers
        if (container is "mp4" or "mov" or "webm" or "mkv")
        {
            return mediaData.OptimizedForStreaming ? ReleaseSource.WebDl : ReleaseSource.WebRip;
        }

        // Fallback
        return ReleaseSource.WebRip;
    }
}
