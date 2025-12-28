namespace Reaparr.BackgroundJobs;

public static class BasePlexMediaDataPartMapper
{
    /// <summary>
    /// Updates the metadata properties of a BasePlexMediaDataPart from a LibraryMediaItemPartDTO.
    /// Extracts video, audio, and subtitle stream information and sets the corresponding properties.
    /// </summary>
    public static void UpdateMetadataFromDTO(this BasePlexMediaDataPart part, LibraryMediaItemPartDTO partDto)
    {
        var streams = partDto.Stream ?? [];

        // Extract video stream info
        var videoStream = streams.FirstOrDefault(s => s.StreamType == StreamType.Video);
        if (videoStream != null)
        {
            part.BitDepth = videoStream.BitDepth ?? 0;
            part.ColorSpace = videoStream.ColorSpace;
            part.FrameRate = Convert.ToDecimal(videoStream.FrameRate);
            part.VideoCodec = videoStream.Codec;
            part.Resolution = videoStream.DisplayTitle;
            // HDR detection
            part.IsDolbyVision = videoStream.DOVIPresent ?? false;
            part.IsHdr10 = videoStream.ColorTrc is "smpte2084" or "arib-std-b67" && !part.IsDolbyVision;
            part.IsHdr = part.IsDolbyVision || part.IsHdr10;
            part.Width = videoStream.Width ?? 0;
            part.Height = videoStream.Height ?? 0;
        }

        // Extract audio stream info
        var audioStreams = streams.Where(s => s.StreamType == StreamType.Audio).ToList();
        if (audioStreams.Count > 0)
        {
            // Primary audio is typically the first or default-selected stream
            var primaryAudio = audioStreams.FirstOrDefault(s => s.Default == true) ?? audioStreams.First();
            part.PrimaryAudioCodec = primaryAudio.Codec;
            part.AudioChannels = primaryAudio.Channels switch
            {
                1 => "1.0",
                2 => "2.0",
                6 => "5.1",
                8 => "7.1",
                _ => "Unknown",
            };

            // Max channels across all audio tracks
            part.MaxAudioChannels = audioStreams.Max(s => s.Channels ?? 0);

            // Atmos detection (usually indicated by codec or extended title)
            part.HasAtmos = audioStreams.Any(s =>
                s.ExtendedDisplayTitle.Contains("Atmos", StringComparison.OrdinalIgnoreCase)
                || s.Title?.Contains("Atmos", StringComparison.OrdinalIgnoreCase) == true
            );

            // Collect distinct audio languages
            var audioLanguages = audioStreams
                .Where(s => !string.IsNullOrEmpty(s.LanguageCode))
                .Select(s => s.LanguageCode)
                .Distinct()
                .ToList();
            part.AudioLanguages = audioLanguages.Count > 0 ? string.Join(",", audioLanguages) : null;
        }

        // Extract subtitle stream info
        var subtitleStreams = streams.Where(s => s.StreamType == StreamType.Subtitle).ToList();
        if (subtitleStreams.Count > 0)
        {
            // Collect distinct subtitle languages
            var subtitleLanguages = subtitleStreams
                .Where(s => !string.IsNullOrEmpty(s.LanguageCode))
                .Select(s => s.LanguageCode)
                .Distinct()
                .ToList();
            part.SubtitleLanguages = subtitleLanguages.Count > 0 ? string.Join(",", subtitleLanguages) : null;

            // SDH/Hearing impaired subs
            part.HasSdhSubs = subtitleStreams.Any(s => s.HearingImpaired == true);

            // Forced subs
            part.HasForcedSubs = subtitleStreams.Any(s => s.Forced == true);
        }

        part.Source = DetermineSource(part);

        // Mark as enriched
        part.HasMetadata = true;
        part.LastSyncedAt = DateTime.UtcNow;
    }

    public static ReleaseSource DetermineSource(BasePlexMediaDataPart part)
    {
        var codec = part.PrimaryAudioCodec?.ToLowerInvariant();
        var container = part.Container?.ToLowerInvariant();
        var height = part.Height;
        var bitrate = part.VideoBitrate;

        // 1. DVD (very distinctive)
        if (height <= 576 && codec is "ac3" or "mp2" && container is "vob" or "mpg" or "mpeg")
        {
            return ReleaseSource.DVD;
        }

        // 2. HDTV (broadcast characteristics)
        if (codec is "ac3" or "mp2" && container is "mpegts" or "ts" && height <= 1080)
        {
            return ReleaseSource.HDTV;
        }

        // 3. Disc audio → BluRay or Remux
        var hasDiscAudio = codec is "dts-hd ma" or "truehd" or "dca-ma";

        if (hasDiscAudio)
        {
            var remuxThresholdKbps = height switch
            {
                >= 2160 => 40_000,
                >= 1080 => 25_000,
                _ => 18_000,
            };

            if (bitrate >= remuxThresholdKbps)
                return ReleaseSource.Remux;

            return ReleaseSource.BluRay;
        }

        // 4. WEB-DL (direct streaming file)
        var isStreamingAudio = codec is "eac3" or "aac" or "opus" or "ac3";

        var isStreamingContainer = container is "mp4" or "mov";

        if (isStreamingAudio && isStreamingContainer)
            return ReleaseSource.WebDl;

        // 5. Fallback → WEBRip
        return ReleaseSource.WebRip;
    }
}
