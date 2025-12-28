using System.Text;
using System.Text.RegularExpressions;

namespace Reaparr.BackgroundJobs;

public static class BasePlexMediaDataPartMapper
{
    /// <summary>
    /// Updates the metadata properties of a BasePlexMediaDataPart from a LibraryMediaItemDTO and LibraryMediaItemPartDTO.
    /// Extracts video, audio, and subtitle stream information and sets the corresponding properties.
    /// </summary>
    public static void UpdateMetadataFromDTO(
        this BasePlexMediaDataPart part,
        LibraryMediaItemDTO mediaItem,
        LibraryMediaItemPartDTO partItem
    )
    {
        var streams = partItem.Stream ?? [];

        // Set OriginalFilename from partItem.File (extract filename only, not full path)
        part.OriginalFilename = partItem.File.GetFileName();

        // Generate the release name and set GeneratedFilename
        part.GeneratedFilename = GenerateReleaseName(mediaItem);

        // Find the Media entry that contains this part
        var containingMedia = mediaItem.Media.FirstOrDefault(m => m.Parts?.Any(p => p.Id == partItem.Id) == true);

        // Extract video stream info
        var videoStream = streams.FirstOrDefault(s => s.StreamType == StreamType.Video);
        if (videoStream != null)
        {
            part.FrameRate = Convert.ToDecimal(videoStream.FrameRate ?? 0);
            part.VideoCodec = videoStream.Codec ?? string.Empty;

            // Format resolution from height (prefer video stream height, fallback to media height)
            var height = videoStream.Height ?? containingMedia?.Height;
            part.Resolution = FormatResolution(height);
        }
        else
        {
            // Fallback if no video stream
            part.FrameRate = 0;
            part.VideoCodec = string.Empty;
            part.Resolution = FormatResolution(containingMedia?.Height);
        }

        // Extract audio stream info
        var audioStreams = streams.Where(s => s.StreamType == StreamType.Audio).ToList();
        var primaryAudio = GetPrimaryAudioStream(audioStreams);

        if (primaryAudio != null)
        {
            part.PrimaryAudioCodec = primaryAudio.Codec ?? string.Empty;
            part.AudioChannels = FormatChannels(primaryAudio.Channels);
        }
        else
        {
            part.PrimaryAudioCodec = string.Empty;
            part.AudioChannels = "Unknown";
        }

        // Determine a source using the new method with access to bitrate information
        var heightForSource = videoStream?.Height ?? containingMedia?.Height;
        var videoBitrate = videoStream?.Bitrate ?? 0;
        var mediaBitrate = containingMedia?.Bitrate ?? 0;
        var audioCodec = primaryAudio?.Codec ?? string.Empty;
        var audioProfile = primaryAudio?.Profile;

        var (source, _) = DetermineReleaseSource(
            audioCodec,
            audioProfile,
            partItem.Container,
            heightForSource,
            videoBitrate,
            mediaBitrate
        );

        part.Source = source;

        // Mark as enriched
        part.HasMetadata = true;
        part.LastSyncedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Generates a Radarr/Sonarr-compatible scene-style release name from Plex movie metadata.
    /// Format: Title.Year.Resolution.Source.[REMUX].VideoCodec.AudioCodec.Channels[.Language]
    /// </summary>
    public static string GenerateReleaseName(LibraryMediaItemDTO movie)
    {
        // Select the highest-resolution Media entry
        var selectedMedia = movie.Media.OrderByDescending(m => m.Height).FirstOrDefault();
        if (selectedMedia == null || selectedMedia.Parts == null || selectedMedia.Parts.Count == 0)
        {
            return string.Empty;
        }

        // Select first Part (or largest by Size)
        var selectedPart = selectedMedia.Parts.OrderByDescending(p => p.Size).FirstOrDefault();
        if (selectedPart == null || selectedPart.Stream == null)
        {
            return string.Empty;
        }

        var streams = selectedPart.Stream;

        // Extract video stream
        var videoStream = streams.FirstOrDefault(s => s.StreamType == StreamType.Video);
        if (videoStream == null)
        {
            return string.Empty;
        }

        // Extract primary audio stream
        var audioStreams = streams.Where(s => s.StreamType == StreamType.Audio).ToList();
        if (audioStreams.Count == 0)
        {
            return string.Empty;
        }

        var primaryAudio = GetPrimaryAudioStream(audioStreams);
        if (primaryAudio == null)
        {
            return string.Empty;
        }

        // Map codecs
        var videoCodec = MapVideoCodec(videoStream.Codec);
        var audioCodec = MapAudioCodec(primaryAudio.Codec, primaryAudio.Profile);

        // Format resolution
        var height = videoStream.Height ?? selectedMedia.Height;
        var resolution = FormatResolution(height);

        // Determine source with REMUX detection
        var (source, isRemux) = DetermineReleaseSource(
            primaryAudio.Codec,
            primaryAudio.Profile,
            selectedPart.Container,
            height,
            videoStream.Bitrate,
            selectedMedia.Bitrate
        );

        // Format channels
        var channels = FormatChannels(primaryAudio.Channels);

        // Format language (omit English, use MULTI if multiple non-English languages)
        var language = FormatLanguage(audioStreams);

        // Detect HDR tokens from video stream
        var hdrTokens = DetectHdrTokens(videoStream);

        // Sanitize title
        var sanitizedTitle = SanitizeTitle(movie.Title);

        // Build release name: Title.Year.Resolution.Source.[REMUX].[HDR/DV].[VideoCodec].AudioCodec.Channels[.Language]
        // Note: VideoCodec is omitted for REMUX releases as remuxes don't imply re-encoding
        var releaseName = new StringBuilder();
        releaseName.Append(sanitizedTitle);
        releaseName.Append('.');
        releaseName.Append(movie.Year);
        releaseName.Append('.');
        releaseName.Append(resolution);
        releaseName.Append('.');
        releaseName.Append(FormatSource(source));
        if (isRemux)
        {
            releaseName.Append(".REMUX");
        }

        // Add HDR tokens after REMUX (if present) and before video codec
        if (hdrTokens.Count > 0)
        {
            foreach (var token in hdrTokens)
            {
                releaseName.Append('.');
                releaseName.Append(token);
            }
        }

        // Only include video codec for non-REMUX releases
        if (!isRemux)
        {
            releaseName.Append('.');
            releaseName.Append(videoCodec);
        }

        releaseName.Append('.');
        releaseName.Append(audioCodec);
        releaseName.Append('.');
        releaseName.Append(channels);
        if (!string.IsNullOrEmpty(language))
        {
            releaseName.Append('.');
            releaseName.Append(language);
        }

        return releaseName.ToString();
    }

    private static string MapVideoCodec(string codec)
    {
        if (string.IsNullOrEmpty(codec))
        {
            return "Unknown";
        }

        var lowerCodec = codec.ToLowerInvariant();
        return lowerCodec switch
        {
            "h264" => "x264",
            "hevc" => "x265",
            "vc1" => "VC-1",
            _ => codec.CapitalizeFirst(),
        };
    }

    private static string MapAudioCodec(string codec, string? profile)
    {
        if (string.IsNullOrEmpty(codec))
        {
            return "Unknown";
        }

        var lowerCodec = codec.ToLowerInvariant();
        var lowerProfile = profile?.ToLowerInvariant() ?? string.Empty;

        // Check for DTS-HD MA variants
        if (lowerCodec == "dca" && lowerProfile.Contains("ma", StringComparison.OrdinalIgnoreCase))
        {
            return "DTS-HD.MA";
        }

        if (lowerCodec is "dts-hd ma" or "dts-hd.ma")
        {
            return "DTS-HD.MA";
        }

        if (lowerCodec is "dts-hd" or "dtshd")
        {
            return "DTS-HD";
        }

        return lowerCodec switch
        {
            "truehd" => "TrueHD",
            "eac3" => "DDP",
            "ac3" => "AC3",
            "aac" => "AAC",
            _ => codec.CapitalizeFirst(),
        };
    }

    private static string FormatResolution(int? height)
    {
        if (!height.HasValue)
        {
            return "Unknown";
        }

        return height.Value switch
        {
            >= 2160 => "2160p",
            >= 1080 => "1080p",
            >= 720 => "720p",
            >= 480 => "480p",
            _ => $"{height.Value}p",
        };
    }

    private static (ReleaseSource source, bool isRemux) DetermineReleaseSource(
        string audioCodec,
        string? audioProfile,
        string container,
        int? height,
        int videoBitrate,
        int mediaBitrate
    )
    {
        var codec = audioCodec?.ToLowerInvariant() ?? string.Empty;
        var profile = audioProfile?.ToLowerInvariant() ?? string.Empty;
        var lowerContainer = container?.ToLowerInvariant() ?? string.Empty;
        var heightValue = height ?? 0;

        // 1. DVD (very distinctive)
        if (heightValue <= 576 && codec is "ac3" or "mp2" && lowerContainer is "vob" or "mpg" or "mpeg")
        {
            return (ReleaseSource.DVD, false);
        }

        // 2. HDTV (broadcast characteristics)
        if (codec is "ac3" or "mp2" && lowerContainer is "mpegts" or "ts" && heightValue <= 1080)
        {
            return (ReleaseSource.HDTV, false);
        }

        // 3. Disc audio → BluRay or Remux
        var hasDiscAudio =
            codec is "dts-hd ma" or "dts-hd.ma" or "truehd" or "dca-ma"
            || (codec == "dca" && profile.Contains("ma", StringComparison.OrdinalIgnoreCase));

        if (hasDiscAudio)
        {
            // Use video bitrate if available, otherwise fallback to media bitrate
            // Bitrate is in bits per second, convert to kbps
            var bitrateKbps = videoBitrate > 0 ? videoBitrate / 1000 : mediaBitrate / 1000;

            var remuxThresholdKbps = heightValue switch
            {
                >= 2160 => 40_000, // 4K threshold
                >= 1080 => 25_000, // 1080p threshold
                _ => 18_000, // 720p and below threshold
            };

            if (bitrateKbps >= remuxThresholdKbps)
            {
                // REMUX is a flag, not a source - source is still BluRay
                return (ReleaseSource.BluRay, true);
            }

            return (ReleaseSource.BluRay, false);
        }

        // 4. WEB-DL (direct streaming file)
        // Note: This heuristic is imperfect - some WEB-DLs are in MKV (e.g., Apple TV, Amazon),
        // and some remuxes are in MP4. We prioritize streaming codecs + streaming containers.
        // WEBRip fallback is used when we can't definitively identify WEB-DL.
        var isStreamingAudio = codec is "eac3" or "aac" or "opus" or "ac3";
        var isStreamingContainer = lowerContainer is "mp4" or "mov";

        if (isStreamingAudio && isStreamingContainer)
        {
            return (ReleaseSource.WebDl, false);
        }

        // 5. Fallback → WEBRip (re-encoded stream capture or ambiguous web source)
        return (ReleaseSource.WebRip, false);
    }

    private static string FormatChannels(int? channels)
    {
        if (!channels.HasValue)
        {
            return "Unknown";
        }

        return channels.Value switch
        {
            1 => "1.0",
            2 => "2.0",
            6 => "5.1",
            8 => "7.1",
            _ => $"{channels.Value}.0",
        };
    }

    private static string FormatLanguage(List<LibraryMediaItemStreamDTO> audioStreams)
    {
        if (audioStreams == null || audioStreams.Count == 0)
        {
            return string.Empty;
        }

        // Extract all non-English language codes
        var nonEnglishLanguages = audioStreams
            .Select(a => a.LanguageCode ?? a.LanguageTag ?? string.Empty)
            .Where(lang => !IsEnglish(lang))
            .Select(lang =>
            {
                var lowerLang = lang.ToLowerInvariant();
                var match = Regex.Match(lowerLang, @"^([a-z]{2,3})");
                return match.Success ? match.Groups[1].Value.ToUpperInvariant() : string.Empty;
            })
            .Where(lang => !string.IsNullOrEmpty(lang))
            .Distinct()
            .ToList();

        if (nonEnglishLanguages.Count == 0)
        {
            return string.Empty;
        }

        // If multiple non-English languages exist, return MULTI
        if (nonEnglishLanguages.Count > 1)
        {
            return "MULTI";
        }

        // Single non-English language
        return nonEnglishLanguages[0];
    }

    private static bool IsEnglish(string lang)
    {
        if (string.IsNullOrEmpty(lang))
        {
            return false;
        }

        var lowerLang = lang.ToLowerInvariant();
        return lowerLang == "en" || lowerLang == "eng" || lowerLang == "english";
    }

    private static string SanitizeTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
        {
            return string.Empty;
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new StringBuilder(title.Length);

        foreach (var c in title)
        {
            // Replace invalid filename characters and spaces with dots
            if (invalidChars.Contains(c) || char.IsWhiteSpace(c))
            {
                sanitized.Append('.');
            }
            else
            {
                sanitized.Append(c);
            }
        }

        // Replace multiple consecutive dots with single dot
        var result = Regex.Replace(sanitized.ToString(), @"\.+", ".");

        // Remove dots from start and end
        result = result.Trim('.');

        return result;
    }

    private static string FormatSource(ReleaseSource source)
    {
        // Note: Remux is handled as a flag (isRemux), not a source enum value
        // When remux is detected, source is BluRay with isRemux=true
        return source switch
        {
            ReleaseSource.BluRay => "BluRay",
            ReleaseSource.WebDl => "WEB-DL",
            ReleaseSource.WebRip => "WEBRip",
            ReleaseSource.DVD => "DVD",
            ReleaseSource.HDTV => "HDTV",
            ReleaseSource.Remux => "BluRay", // Legacy support - should not occur with new logic
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Gets the primary audio stream using priority: Selected+Default > Default > First available.
    /// </summary>
    private static LibraryMediaItemStreamDTO? GetPrimaryAudioStream(List<LibraryMediaItemStreamDTO> audioStreams)
    {
        if (audioStreams.Count == 0)
        {
            return null;
        }

        return audioStreams.FirstOrDefault(s => s.Selected == true && s.Default == true)
            ?? audioStreams.FirstOrDefault(s => s.Default == true)
            ?? audioStreams.First();
    }

    /// <summary>
    /// Detects HDR tokens from video stream metadata.
    /// Only returns tokens when explicitly detected - never infers from bit depth alone.
    /// Returns list of tokens in order: DV (if present), then HDR/HDR10+ (if present).
    /// </summary>
    private static List<string> DetectHdrTokens(LibraryMediaItemStreamDTO? videoStream)
    {
        var tokens = new List<string>();

        if (videoStream == null)
        {
            return tokens;
        }

        var colorPrimaries = videoStream.ColorPrimaries?.ToLowerInvariant() ?? string.Empty;
        var colorTrc = videoStream.ColorTrc?.ToLowerInvariant() ?? string.Empty;
        var profile = videoStream.Profile?.ToLowerInvariant() ?? string.Empty;
        var hasDolbyVision = videoStream.DOVIPresent == true;

        // Detect Dolby Vision
        var hasDv = hasDolbyVision || profile.Contains("dv", StringComparison.OrdinalIgnoreCase);

        // Detect HDR10/HDR10+
        // Require BT.2020 color primaries and PQ transfer function (SMPTE 2084) for reliable HDR detection
        var hasBt2020 = colorPrimaries == "bt2020" || colorPrimaries.Contains("bt.2020", StringComparison.OrdinalIgnoreCase);
        var hasPq = colorTrc == "smpte2084" || colorTrc.Contains("pq", StringComparison.OrdinalIgnoreCase);
        var hasHdrProfile = profile.Contains("hdr", StringComparison.OrdinalIgnoreCase);
        var hasHdr10Plus = profile.Contains("hdr10+", StringComparison.OrdinalIgnoreCase);

        // HDR detection: require explicit signals (BT.2020 + PQ) or HDR in profile
        // Do NOT infer from bit depth alone
        var hasHdr = (hasBt2020 && hasPq) || hasHdrProfile;

        // Build tokens in correct order
        if (hasDv)
        {
            tokens.Add("DV");

            // DV.HDR if both Dolby Vision and HDR10 fallback are present
            if (hasHdr && !hasHdr10Plus)
            {
                tokens.Add("HDR");
            }
        }
        else if (hasHdr)
        {
            // HDR10+ takes precedence over HDR10
            if (hasHdr10Plus)
            {
                tokens.Add("HDR10+");
            }
            else
            {
                tokens.Add("HDR");
            }
        }

        return tokens;
    }
}
