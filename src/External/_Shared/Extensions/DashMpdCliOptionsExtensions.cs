using System.Text;
using Reaparr.External.Contracts;

namespace Reaparr.External;

public static class DashMpdCliOptionsExtensions
{
    public static string ToBuildArguments(this DashMpdCliOptions options, string mpdUrl, string outputPath)
    {
        var args = new StringBuilder();

        // Output file
        args.Append($"--output \"{outputPath}\"");

        // Verbosity
        if (options.Quiet)
        {
            args.Append(" --quiet");
        }
        else if (options.Verbose)
        {
            args.Append(" -v -v");
        }

        // Custom headers
        if (options.Headers != null)
        {
            foreach (var (key, value) in options.Headers)
            {
                args.Append($" --add-header \"{key}: {value}\"");
            }
        }

        // Proxy configuration
        if (!string.IsNullOrWhiteSpace(options.Proxy))
        {
            args.Append($" --proxy \"{options.Proxy}\"");
        }

        if (options.NoProxy)
        {
            args.Append(" --no-proxy");
        }

        // Bandwidth control
        if (!string.IsNullOrWhiteSpace(options.LimitRate))
        {
            args.Append($" --limit-rate {options.LimitRate}");
        }

        // Authentication
        if (!string.IsNullOrWhiteSpace(options.AuthUsername))
        {
            args.Append($" --auth-username \"{options.AuthUsername}\"");
        }

        if (!string.IsNullOrWhiteSpace(options.AuthPassword))
        {
            args.Append($" --auth-password \"{options.AuthPassword}\"");
        }

        if (!string.IsNullOrWhiteSpace(options.AuthBearer))
        {
            args.Append($" --auth-bearer \"{options.AuthBearer}\"");
        }

        // Advanced options
        if (options.EnableLiveStreams)
        {
            args.Append(" --enable-live-streams");
        }

        if (options.SleepRequests.HasValue)
        {
            args.Append($" --sleep-requests {options.SleepRequests.Value}");
        }

        if (!string.IsNullOrWhiteSpace(options.CookiesFromBrowser))
        {
            args.Append($" --cookies-from-browser {options.CookiesFromBrowser}");
        }

        // Decryption (DRM)
        if (options.DecryptionKeys != null)
        {
            foreach (var key in options.DecryptionKeys)
            {
                args.Append($" --key \"{key}\"");
            }
        }

        if (!string.IsNullOrWhiteSpace(options.DecryptionApplication))
        {
            args.Append($" --decryption-application {options.DecryptionApplication}");
        }

        if (!string.IsNullOrWhiteSpace(options.Mp4DecryptLocation))
        {
            args.Append($" --mp4decrypt-location \"{options.Mp4DecryptLocation}\"");
        }

        if (!string.IsNullOrWhiteSpace(options.ShakaPackagerLocation))
        {
            args.Append($" --shaka-packager-location \"{options.ShakaPackagerLocation}\"");
        }

        // Muxing
        if (options.MuxerPreference != null)
        {
            foreach (var (container, preference) in options.MuxerPreference)
            {
                args.Append($" --muxer-preference {container}:{preference}");
            }
        }

        if (!string.IsNullOrWhiteSpace(options.FfmpegLocation))
        {
            args.Append($" --ffmpeg-location \"{options.FfmpegLocation}\"");
        }

        if (!string.IsNullOrWhiteSpace(options.VlcLocation))
        {
            args.Append($" --vlc-location \"{options.VlcLocation}\"");
        }

        if (!string.IsNullOrWhiteSpace(options.MkvmergeLocation))
        {
            args.Append($" --mkvmerge-location \"{options.MkvmergeLocation}\"");
        }

        if (!string.IsNullOrWhiteSpace(options.Mp4BoxLocation))
        {
            args.Append($" --mp4box-location \"{options.Mp4BoxLocation}\"");
        }

        // Quality selection
        if (!string.IsNullOrWhiteSpace(options.Quality))
        {
            args.Append($" --quality \"{options.Quality}\"");
        }

        if (options.PreferVideoHeight.HasValue)
        {
            args.Append($" --prefer-video-height {options.PreferVideoHeight.Value}");
        }

        if (options.PreferVideoWidth.HasValue)
        {
            args.Append($" --prefer-video-width {options.PreferVideoWidth.Value}");
        }

        if (!string.IsNullOrWhiteSpace(options.AudioLanguage))
        {
            args.Append($" --prefer-language \"{options.AudioLanguage}\"");
        }

        // Other options
        if (!string.IsNullOrWhiteSpace(options.DropElements))
        {
            args.Append($" --drop-elements \"{options.DropElements}\"");
        }

        if (!string.IsNullOrWhiteSpace(options.XsltStylesheet))
        {
            args.Append($" --xslt-stylesheet \"{options.XsltStylesheet}\"");
        }

        if (options.NoPeriodConcatenation)
        {
            args.Append(" --no-period-concatenation");
        }

        // Additional custom arguments
        if (!string.IsNullOrWhiteSpace(options.AdditionalArguments))
        {
            args.Append($" {options.AdditionalArguments}");
        }

        // Always use NDJSON output
        args.Append(" --progress=json");

        // MPD URL (must be last)
        args.Append($" \"{mpdUrl}\"");

        return args.ToString();
    }
}
