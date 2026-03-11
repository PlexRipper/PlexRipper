using Flurl;
using LukeHagar.PlexAPI.SDK.Models.Requests;

namespace Reaparr.PlexApi;

public static class DashTranscodeUrlExtensions
{
    public static Url ApplyDashTranscodeQueryParams(this Url url, MakeDecisionRequest request, string token) =>
        url.SetQueryParam("hasMDE", request.HasMDE)
            .SetQueryParam("path", request.Path)
            .SetQueryParam("mediaIndex", request.MediaIndex)
            .SetQueryParam("partIndex", request.PartIndex)
            .SetQueryParam("protocol", request.Protocol)
            .SetQueryParam("directPlay", request.DirectPlay)
            .SetQueryParam("directStream", request.DirectStream)
            .SetQueryParam("directStreamAudio", request.DirectStreamAudio)
            .SetQueryParam("subtitleSize", request.SubtitleSize)
            .SetQueryParam("audioBoost", request.AudioBoost)
            .SetQueryParam("location", request.Location)
            .SetQueryParam("autoAdjustQuality", request.AutoAdjustQuality)
            .SetQueryParam("autoAdjustSubtitle", request.AutoAdjustSubtitle)
            .SetQueryParam("maxVideoBitrate", request.PeakBitrate)
            .SetQueryParam("mediaBufferSize", request.MediaBufferSize)
            .SetQueryParam("session", request.TranscodeSessionId)
            .SetQueryParam("subtitles", request.Subtitles)
            .SetQueryParam("videoResolution", request.VideoResolution)
            .SetQueryParam("videoQuality", request.VideoQuality)
            .SetQueryParam("X-Plex-Session-Identifier", request.XPlexSessionIdentifier)
            .SetQueryParam("X-Plex-Client-Profile-Extra", request.XPlexClientProfileExtra)
            .SetQueryParam("X-Plex-Product", request.Product)
            .SetQueryParam("X-Plex-Version", request.Version)
            .SetQueryParam("X-Plex-Client-Identifier", request.ClientIdentifier)
            .SetQueryParam("X-Plex-Platform", request.Platform)
            .SetQueryParam("X-Plex-Platform-Version", request.PlatformVersion)
            .SetQueryParam("X-Plex-Model", request.Model)
            .SetQueryParam("X-Plex-Device", request.Device)
            .SetQueryParam("X-Plex-Device-Name", request.DeviceName)
            .SetQueryParam("X-Plex-Token", token);
}
