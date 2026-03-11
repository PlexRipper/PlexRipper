using FastEndpoints;
using FluentValidation;
using Flurl;
using LukeHagar.PlexAPI.SDK.Models.Components;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using Reaparr.Data.Contracts;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.PlexApi;

public class GetTranscodeUrlCommandValidator : AbstractValidator<GetTranscodeUrlCommand>
{
    public GetTranscodeUrlCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.DownloadTaskKey).NotNull();
        RuleFor(x => x.DownloadTaskKey.IsValid).Equal(true);
        RuleFor(x => x.MetaDataPath).NotEmpty();
        RuleFor(x => x.MetaDataPath).Must(x => x.Contains("/library/metadata/"));
    }
}

public class GetTranscodeUrlCommandHandler : ICommandHandler<GetTranscodeUrlCommand, Result<GetTranscodeUrlResult>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    private static readonly string _clientIdentifier = GenerateClientId();

    private const string CLIENT_PROFILE_EXTRA =
        "add-direct-play-profile(type=videoProfile&videoCodec=*&audioCodec=*&container=*)"
        + "+append-transcode-target-codec(type=videoProfile&context=streaming"
        + "&videoCodec=h264,hevc,vp9,av1,mpeg2video,mpeg4,vc1"
        + "&audioCodec=aac,ac3,eac3,dts,dca,mp3,flac,opus,vorbis,truehd"
        + "&protocol=http)"
        + "+add-limitation(scope=videoCodec&scopeName=*&type=upperBound&name=video.bitDepth&value=12&isRequired=false)"
        + "+add-limitation(scope=videoCodec&scopeName=*&type=upperBound&name=video.width&value=3840&isRequired=false)"
        + "+add-limitation(scope=videoCodec&scopeName=*&type=upperBound&name=video.height&value=2160&isRequired=false)"
        + "+add-limitation(scope=videoCodec&scopeName=*&type=upperBound&name=video.bitrate&value=2000000&isRequired=false)";

    public GetTranscodeUrlCommandHandler(ILogger logger, IReaparrDbContext dbContext, ICommandExecutor commandExecutor)
    {
        _log = logger.ForContext<GetTranscodeUrlCommandHandler>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public async Task<Result<GetTranscodeUrlResult>> ExecuteAsync(
        GetTranscodeUrlCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexServerId = command.DownloadTaskKey.PlexServerId;

        var plexServerConnectionResult = await _dbContext.ChoosePlexServerConnection(plexServerId, cancellationToken);
        if (plexServerConnectionResult.IsFailed)
            return plexServerConnectionResult.ToResult().LogError();

        var plexServerConnection = plexServerConnectionResult.Value;
        var plexServer = plexServerConnection.PlexServer;

        var tokenResult = await _dbContext.GetPlexServerTokenAsync(plexServerId, cancellationToken);
        if (tokenResult.IsFailed)
        {
            _log.Here().Error("Could not find a valid token for server {ServerName}", plexServer?.Name ?? "Unknown");
            return tokenResult.ToResult();
        }

        var token = tokenResult.Value;

        var transcodeSessionId = GenerateSessionId();
        var plexSessionId = GenerateSessionId();
        var playbackSessionId = Guid.NewGuid().ToString();
        var playbackId = Guid.NewGuid().ToString();

        var decisionRequest = new MakeDecisionRequest
        {
            Accepts = Accepts.ApplicationJson,
            ClientIdentifier = _clientIdentifier,
            Product = "Plex for Roku",
            Version = "7.6.0",
            Platform = "Roku",
            PlatformVersion = "14.0.0",
            Device = "Roku Ultra",
            Model = "4802RW",
            DeviceName = "Roku Ultra",
            TranscodeType = TranscodeType.Video,
            HasMDE = BoolInt.True,
            Path = command.MetaDataPath,
            MediaIndex = 0,
            PartIndex = 0,
            Protocol = LukeHagar.PlexAPI.SDK.Models.Requests.Protocol.Http,
            DirectPlay = BoolInt.True,
            DirectStream = BoolInt.True,
            DirectStreamAudio = BoolInt.True,
            SubtitleSize = 100,
            AudioBoost = 100,
            Location = LukeHagar.PlexAPI.SDK.Models.Requests.Location.Lan,
            AutoAdjustQuality = BoolInt.False,
            AutoAdjustSubtitle = BoolInt.True,
            PeakBitrate = 2000000,
            MediaBufferSize = 102400,
            Subtitles = LukeHagar.PlexAPI.SDK.Models.Requests.Subtitles.None,
            VideoResolution = "3840x2160",
            VideoQuality = 100,
            XPlexClientProfileExtra = CLIENT_PROFILE_EXTRA,
            XPlexSessionIdentifier = plexSessionId,
            TranscodeSessionId = transcodeSessionId,
        };

        var decisionResult = await _commandExecutor.Send(
            new GetDashTranscodeDecisionCommand(plexServerId, decisionRequest),
            cancellationToken
        );

        if (decisionResult.IsFailed)
            return decisionResult.ToResult().LogError();

        var decisionSummary = decisionResult.Value;

        _log.Here()
            .Information(
                "Decision returned: generalCode={GeneralCode}, generalText={GeneralText}, "
                    + "transcodeCode={TranscodeCode}, transcodeText={TranscodeText}, "
                    + "session={Session}, sessionId={SessionId}",
                decisionSummary.GeneralDecisionCode,
                decisionSummary.GeneralDecisionText,
                decisionSummary.TranscodeDecisionCode,
                decisionSummary.TranscodeDecisionText,
                transcodeSessionId,
                plexSessionId
            );

        _log.Here()
            .Information(
                "Stream decisions - Video: {VideoDecision}, Audio: {AudioDecision}, TranscodedQuality: {TranscodedQuality}",
                decisionSummary.VideoDecision,
                decisionSummary.AudioDecision,
                decisionSummary.TranscodedQuality
            );

        if (!string.Equals(decisionSummary.VideoDecision, "copy", StringComparison.OrdinalIgnoreCase))
        {
            _log.Here()
                .Warning(
                    "Video is being transcoded instead of direct streamed. "
                        + "Quality may be degraded. Decision: {Decision}",
                    decisionSummary.VideoDecision
                );
        }

        if (!string.Equals(decisionSummary.AudioDecision, "copy", StringComparison.OrdinalIgnoreCase))
        {
            _log.Here()
                .Warning(
                    "Audio is being transcoded instead of direct streamed. "
                        + "Quality may be degraded. Decision: {Decision}",
                    decisionSummary.AudioDecision
                );
        }

        var downloadUrl = new Url(plexServerConnection.Url)
            .AppendPathSegment("video/:/transcode/universal/start.mpd")
            .ApplyDashTranscodeQueryParams(decisionRequest, token)
            .SetQueryParam("fastSeek", 1)
            .SetQueryParam("addDebugOverlay", 0)
            .SetQueryParam("Accept-Language", "en")
            .SetQueryParam("X-Plex-Incomplete-Segments", 1)
            .SetQueryParam("X-Plex-Features", "external-media,indirect-media,hub-style-list")
            .SetQueryParam("X-Plex-Device-Screen-Resolution", "3840x2160,3840x2160")
            .SetQueryParam("X-Plex-Language", "en")
            .SetQueryParam("X-Plex-Session-Id", playbackSessionId)
            .SetQueryParam("X-Plex-Playback-Session-Id", playbackSessionId)
            .SetQueryParam("X-Plex-Playback-Id", playbackId)
            .ToString();

        return Result.Ok(
            new GetTranscodeUrlResult
            {
                DownloadUrl = downloadUrl,
                TranscodedQuality = decisionSummary.TranscodedQuality,
            }
        );
    }

    private static string GenerateSessionId() => Guid.NewGuid().ToString("N")[..24];

    private static string GenerateClientId() => $"{Guid.NewGuid():N}"[..25];
}
