using FastEndpoints;
using FluentValidation;
using Flurl;
using LukeHagar.PlexAPI.SDK.Models.Components;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using Reaparr.Data.Contracts;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.PlexApi;

public class GetDashTranscodeDecisionCommandValidator : AbstractValidator<GetDashTranscodeDecisionCommand>
{
    public GetDashTranscodeDecisionCommandValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
        RuleFor(x => x.MetaDataPath).NotEmpty();
        RuleFor(x => x.MetaDataPath).Must(path => path.Contains("/library/metadata/"));
        RuleFor(x => x.Session).NotEmpty();
        RuleFor(x => x.SessionIdentifier).NotEmpty();
        RuleFor(x => x.PlaybackSessionId).NotEmpty();
        RuleFor(x => x.PlaybackId).NotEmpty();
    }
}

public class GetDashTranscodeDecisionCommandHandler
    : ICommandHandler<GetDashTranscodeDecisionCommand, Result<GetDashTranscodeDecisionResult>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IPlexApiClientFactory _plexApiClientFactory;

    private const string ClientProfileExtra =
        "append-transcode-target-codec(type=videoProfile&context=streaming&videoCodec=h264%2Chevc&audioCodec=aac&protocol=dash)";

    public GetDashTranscodeDecisionCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        IPlexApiClientFactory plexApiClientFactory
    )
    {
        _log = log.ForContext<GetDashTranscodeDecisionCommandHandler>();
        _dbContext = dbContext;
        _plexApiClientFactory = plexApiClientFactory;
    }

    public async Task<Result<GetDashTranscodeDecisionResult>> ExecuteAsync(
        GetDashTranscodeDecisionCommand command,
        CancellationToken cancellationToken
    )
    {
        var tokenResult = await _dbContext.GetPlexServerTokenAsync(command.PlexServerId, cancellationToken);
        if (tokenResult.IsFailed)
            return tokenResult.ToResult();

        var connectionResult = await _dbContext.ChoosePlexServerConnection(command.PlexServerId, cancellationToken);
        if (connectionResult.IsFailed)
            return connectionResult.ToResult();

        var client = _plexApiClientFactory.CreateClient(
            tokenResult.Value,
            new PlexApiClientOptions
            {
                ConnectionUrl = connectionResult.Value.Url,
                Timeout = 30,
                RetryCount = 3,
            }
        );

        var decisionRequest = new MakeDecisionRequest
        {
            Accepts = Accepts.ApplicationJson,
            ClientIdentifier = command.ClientIdentifier,
            Product = "Plex Web",
            Version = "4.158.0",
            Platform = "Firefox",
            PlatformVersion = "147.0",
            Device = "Linux",
            Model = "standalone",
            DeviceName = "Firefox",
            TranscodeType = TranscodeType.Video,
            HasMDE = BoolInt.True,
            Path = command.MetaDataPath,
            MediaIndex = 0,
            PartIndex = 0,
            Protocol = LukeHagar.PlexAPI.SDK.Models.Requests.Protocol.Dash,
            DirectPlay = BoolInt.False,
            DirectStream = BoolInt.True,
            DirectStreamAudio = BoolInt.True,
            SubtitleSize = 100,
            AudioBoost = 100,
            Location = LukeHagar.PlexAPI.SDK.Models.Requests.Location.Lan,
            AutoAdjustQuality = BoolInt.False,
            AutoAdjustSubtitle = BoolInt.True,
            PeakBitrate = 200000,
            MediaBufferSize = 102400,
            Subtitles = LukeHagar.PlexAPI.SDK.Models.Requests.Subtitles.None,
            VideoResolution = "3840x2160",
            VideoQuality = 100,
            XPlexClientProfileExtra = ClientProfileExtra,
            XPlexSessionIdentifier = command.SessionIdentifier,
            TranscodeSessionId = command.Session,
        };

        var debugDecisionUrl = BuildDecisionDebugUrl(connectionResult.Value.Url, decisionRequest, tokenResult.Value);
        _log.Here().Information("Requesting Plex transcode decision URL: {Url}", debugDecisionUrl);

        var decisionResponse = await client.Transcoder.MakeDecisionAsync(decisionRequest).ToResponse();

        if (decisionResponse.IsFailed)
            return decisionResponse.ToResult();

        var mediaContainer = decisionResponse.Value.MediaContainerWithDecision?.MediaContainer;
        if (mediaContainer is null)
            return Result.Fail("Invalid decision response: missing MediaContainer").LogError();

        var summary = new GetDashTranscodeDecisionResult
        {
            GeneralDecisionCode = mediaContainer.GeneralDecisionCode?.ToString() ?? "unknown",
            GeneralDecisionText = mediaContainer.GeneralDecisionText ?? "unknown",
            TranscodeDecisionCode = mediaContainer.TranscodeDecisionCode?.ToString() ?? "unknown",
            TranscodeDecisionText = mediaContainer.TranscodeDecisionText ?? "unknown",
            VideoDecision = "unknown",
            AudioDecision = "unknown",
            TranscodedQuality = VideoQuality.None,
        };

        foreach (var metadata in mediaContainer.Metadata ?? [])
        {
            foreach (var media in metadata.Media ?? [])
            {
                if (summary.TranscodedQuality is VideoQuality.None)
                {
                    summary = summary with { TranscodedQuality = ResolveMediaQuality(media) };
                }
                else
                {
                    summary = summary with
                    {
                        TranscodedQuality = GetHighestVideoQuality(
                            summary.TranscodedQuality,
                            ResolveMediaQuality(media)
                        ),
                    };
                }

                foreach (var part in media.Part ?? [])
                {
                    foreach (var stream in part.Stream ?? [])
                    {
                        var decision = stream.Decision?.ToString() ?? "unknown";
                        if (IsVideoStream(stream.StreamType))
                        {
                            summary = summary with { VideoDecision = decision };
                            if (summary.TranscodedQuality is VideoQuality.None)
                            {
                                summary = summary with { TranscodedQuality = ResolveStreamQuality(stream, media) };
                            }
                            else
                            {
                                summary = summary with
                                {
                                    TranscodedQuality = GetHighestVideoQuality(
                                        summary.TranscodedQuality,
                                        ResolveStreamQuality(stream, media)
                                    ),
                                };
                            }
                        }
                        else if (IsAudioStream(stream.StreamType))
                        {
                            summary = summary with { AudioDecision = decision };
                        }
                    }
                }
            }
        }

        return Result.Ok(summary);
    }

    private static bool IsVideoStream(MediaContainerWithDecisionStreamType streamType) =>
        streamType.ToString() is "Video" or "1";

    private static bool IsAudioStream(MediaContainerWithDecisionStreamType streamType) =>
        streamType.ToString() is "Audio" or "2";

    private static VideoQuality ResolveStreamQuality(
        MediaContainerWithDecisionStream stream,
        MediaContainerWithDecisionMedia media
    )
    {
        var displayTitleQuality = stream.DisplayTitle?.ToVideoQuality() ?? VideoQuality.Unknown;
        if (displayTitleQuality is not VideoQuality.Unknown)
            return displayTitleQuality;

        var extendedDisplayTitleQuality = stream.ExtendedDisplayTitle?.ToVideoQuality() ?? VideoQuality.Unknown;
        if (extendedDisplayTitleQuality is not VideoQuality.Unknown)
            return extendedDisplayTitleQuality;

        var mediaResolutionQuality = media.VideoResolution?.ToVideoQuality() ?? VideoQuality.Unknown;
        if (mediaResolutionQuality is not VideoQuality.Unknown)
            return mediaResolutionQuality;

        if (stream.Width is { } streamWidth)
            return ToVideoQualityByWidth(Convert.ToInt32(streamWidth));

        if (stream.Height is { } streamHeight)
            return ToVideoQuality(Convert.ToInt32(streamHeight));

        return VideoQuality.None;
    }

    private static VideoQuality ResolveMediaQuality(MediaContainerWithDecisionMedia media)
    {
        var mediaResolutionQuality = media.VideoResolution?.ToVideoQuality() ?? VideoQuality.Unknown;
        if (mediaResolutionQuality is not VideoQuality.Unknown)
            return mediaResolutionQuality;

        if (media.Width is { } mediaWidth)
            return ToVideoQualityByWidth(Convert.ToInt32(mediaWidth));

        if (media.Height is { } mediaHeight)
            return ToVideoQuality(Convert.ToInt32(mediaHeight));

        return VideoQuality.None;
    }

    private static VideoQuality ToVideoQualityByWidth(int width)
    {
        if (width >= 7600)
            return VideoQuality.UHD_8K;
        if (width >= 3800)
            return VideoQuality.UHD_4K;
        if (width >= 2500)
            return VideoQuality.QHD;
        if (width >= 1700)
            return VideoQuality.FullHD;
        if (width >= 1200)
            return VideoQuality.HD;
        if (width >= 950)
            return VideoQuality.DVD;
        if (width >= 700)
            return VideoQuality.SD;
        if (width >= 450)
            return VideoQuality.nHD;
        if (width >= 300)
            return VideoQuality.SubSD_CIF;
        return VideoQuality.SubSD_144p;
    }

    private static VideoQuality ToVideoQuality(int height)
    {
        if (height >= 3800)
            return VideoQuality.UHD_8K;
        if (height >= 2000)
            return VideoQuality.UHD_4K;
        if (height >= 1300)
            return VideoQuality.QHD;
        if (height >= 1000)
            return VideoQuality.FullHD;
        if (height >= 700)
            return VideoQuality.HD;
        if (height >= 560)
            return VideoQuality.DVD;
        if (height >= 470)
            return VideoQuality.SD;
        if (height >= 350)
            return VideoQuality.nHD;
        if (height >= 200)
            return VideoQuality.SubSD_CIF;
        return VideoQuality.SubSD_144p;
    }

    private static VideoQuality GetHighestVideoQuality(VideoQuality current, VideoQuality candidate) =>
        (VideoQuality)Math.Max((int)current, (int)candidate);

    private static string BuildDecisionDebugUrl(string connectionUrl, MakeDecisionRequest request, string token) =>
        new Url(connectionUrl)
            .AppendPathSegment("video/:/transcode/universal/decision")
            .SetQueryParam("hasMDE", 1)
            .SetQueryParam("path", request.Path)
            .SetQueryParam("mediaIndex", request.MediaIndex)
            .SetQueryParam("partIndex", request.PartIndex)
            .SetQueryParam("protocol", "dash")
            .SetQueryParam("directPlay", 0)
            .SetQueryParam("directStream", 1)
            .SetQueryParam("directStreamAudio", 1)
            .SetQueryParam("subtitleSize", request.SubtitleSize)
            .SetQueryParam("audioBoost", request.AudioBoost)
            .SetQueryParam("location", "lan")
            .SetQueryParam("autoAdjustQuality", 0)
            .SetQueryParam("autoAdjustSubtitle", 1)
            .SetQueryParam("maxVideoBitrate", request.PeakBitrate)
            .SetQueryParam("mediaBufferSize", request.MediaBufferSize)
            .SetQueryParam("session", request.TranscodeSessionId)
            .SetQueryParam("subtitles", "none")
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
            .SetQueryParam("X-Plex-Token", token)
            .ToString();
}
