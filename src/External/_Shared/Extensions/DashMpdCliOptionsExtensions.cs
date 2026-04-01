using System.Text;

namespace Reaparr.External;

public static class DashMpdCliOptionsExtensions
{
    public static string ToBuildArguments(this DashMpdCliOptions options)
    {
        var args = new StringBuilder();

        // Output file
        args.Append($"--output \"{Escape(options.Output)}\"");

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
                args.Append($" --add-header \"{Escape(key)}: {Escape(value)}\"");
            }
        }

        // Proxy configuration
        if (!string.IsNullOrWhiteSpace(options.Proxy))
        {
            args.Append($" --proxy \"{Escape(options.Proxy)}\"");
        }

        if (options.NoProxy)
        {
            args.Append(" --no-proxy");
        }

        // Bandwidth control
        if (!string.IsNullOrWhiteSpace(options.LimitRate))
        {
            args.Append($" --limit-rate \"{Escape(options.LimitRate)}\"");
        }

        // Authentication
        if (!string.IsNullOrWhiteSpace(options.AuthUsername))
        {
            args.Append($" --auth-username \"{Escape(options.AuthUsername)}\"");
        }

        if (!string.IsNullOrWhiteSpace(options.AuthPassword))
        {
            args.Append($" --auth-password \"{Escape(options.AuthPassword)}\"");
        }

        if (!string.IsNullOrWhiteSpace(options.AuthBearer))
        {
            args.Append($" --auth-bearer \"{Escape(options.AuthBearer)}\"");
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
            args.Append($" --cookies-from-browser \"{Escape(options.CookiesFromBrowser)}\"");
        }

        // Decryption (DRM)
        if (options.DecryptionKeys != null)
        {
            foreach (var key in options.DecryptionKeys.Where(k => !string.IsNullOrWhiteSpace(k)))
            {
                args.Append($" --key \"{Escape(key)}\"");
            }
        }

        if (!string.IsNullOrWhiteSpace(options.DecryptionApplication))
        {
            args.Append($" --decryption-application \"{Escape(options.DecryptionApplication)}\"");
        }

        if (!string.IsNullOrWhiteSpace(options.Mp4DecryptLocation))
        {
            args.Append($" --mp4decrypt-location \"{Escape(options.Mp4DecryptLocation)}\"");
        }

        if (!string.IsNullOrWhiteSpace(options.ShakaPackagerLocation))
        {
            args.Append($" --shaka-packager-location \"{Escape(options.ShakaPackagerLocation)}\"");
        }

        // Muxing
        if (options.MuxerPreference != null)
        {
            foreach (var (container, preference) in options.MuxerPreference)
            {
                args.Append($" --muxer-preference \"{Escape(container)}:{Escape(preference)}\"");
            }
        }

        if (!string.IsNullOrWhiteSpace(options.FfmpegLocation))
        {
            args.Append($" --ffmpeg-location \"{Escape(options.FfmpegLocation)}\"");
        }

        if (!string.IsNullOrWhiteSpace(options.VlcLocation))
        {
            args.Append($" --vlc-location \"{Escape(options.VlcLocation)}\"");
        }

        if (!string.IsNullOrWhiteSpace(options.MkvmergeLocation))
        {
            args.Append($" --mkvmerge-location \"{Escape(options.MkvmergeLocation)}\"");
        }

        if (!string.IsNullOrWhiteSpace(options.Mp4BoxLocation))
        {
            args.Append($" --mp4box-location \"{Escape(options.Mp4BoxLocation)}\"");
        }

        // Quality selection
        if (!string.IsNullOrWhiteSpace(options.Quality))
        {
            args.Append($" --quality \"{Escape(options.Quality)}\"");
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
            args.Append($" --prefer-language \"{Escape(options.AudioLanguage)}\"");
        }

        // Other options
        if (!string.IsNullOrWhiteSpace(options.DropElements))
        {
            args.Append($" --drop-elements \"{Escape(options.DropElements)}\"");
        }

        if (!string.IsNullOrWhiteSpace(options.XsltStylesheet))
        {
            args.Append($" --xslt-stylesheet \"{Escape(options.XsltStylesheet)}\"");
        }

        if (options.NoPeriodConcatenation)
        {
            args.Append(" --no-period-concatenation");
        }

        // Always use NDJSON output (must be set before AdditionalArguments so it cannot be overridden)
        args.Append(" --progress=json");

        // Additional custom arguments
        if (!string.IsNullOrWhiteSpace(options.AdditionalArguments))
        {
            args.Append($" {options.AdditionalArguments}");
        }

        // MPD URL (must be last)
        args.Append($" \"{Escape(options.MpdUrl)}\"");

        return args.ToString();
    }

    private static string Escape(string value) => value.Replace("\"", "\\\"");
}
