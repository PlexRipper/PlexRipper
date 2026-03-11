using FastEndpoints;
using FluentValidation;
using Flurl;
using Reaparr.Data.Contracts;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.Application;

public record GetDashDownloadUrlCommand : ICommand<Result<DashDownloadUrlResult>>
{
    public required DownloadTaskKey DownloadTaskKey { get; init; }

    public required string MetaDataPath { get; init; }
}

public record DashDownloadUrlResult
{
    public required string DownloadUrl { get; init; }

    public VideoQuality TranscodedQuality { get; init; }
}

public class GetDashDownloadUrlCommandValidator : AbstractValidator<GetDashDownloadUrlCommand>
{
    public GetDashDownloadUrlCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.DownloadTaskKey).NotNull();
        RuleFor(x => x.DownloadTaskKey.IsValid).Equal(true);
        RuleFor(x => x.MetaDataPath).NotEmpty();
        RuleFor(x => x.MetaDataPath).Must(x => x.Contains("/library/metadata/"));
    }
}

public class GetDashDownloadUrlCommandHandler
    : ICommandHandler<GetDashDownloadUrlCommand, Result<DashDownloadUrlResult>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    // Stable client identifier per app install (generate once, reuse forever)
    private static readonly string _clientIdentifier = GenerateClientId();

    private const string CLIENT_PROFILE_EXTRA =
        "append-transcode-target-codec(type=videoProfile&context=streaming&videoCodec=h264%2Chevc&audioCodec=aac&protocol=dash)";

    public GetDashDownloadUrlCommandHandler(
        ILogger logger,
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor
    )
    {
        _log = logger.ForContext<GetDashDownloadUrlCommandHandler>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public async Task<Result<DashDownloadUrlResult>> ExecuteAsync(
        GetDashDownloadUrlCommand command,
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

        // Generate unique session identifiers (must match between decision and start.mpd)
        var session = GenerateSessionId();
        var sessionIdentifier = GenerateSessionId();
        var playbackSessionId = Guid.NewGuid().ToString();
        var playbackId = Guid.NewGuid().ToString();

        var decisionResult = await _commandExecutor.Send(
            new GetDashTranscodeDecisionCommand(
                plexServerId,
                command.MetaDataPath,
                _clientIdentifier,
                session,
                sessionIdentifier,
                playbackSessionId,
                playbackId
            ),
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
                session,
                sessionIdentifier
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

        // Step 2: Build start.mpd URL with byte-identical parameters
        var downloadUrl = BuildQueryParams(
                new Url(plexServerConnection.Url).AppendPathSegment("video/:/transcode/universal/start.mpd"),
                command.MetaDataPath,
                session,
                sessionIdentifier,
                playbackSessionId,
                playbackId,
                token
            )
            .ToString();

        return Result.Ok(
            new DashDownloadUrlResult
            {
                DownloadUrl = downloadUrl,
                TranscodedQuality = decisionSummary.TranscodedQuality,
            }
        );
    }

    /// <summary>
    /// Applies the complete parameter set required for both decision and start.mpd.
    /// Parameters must be byte-identical between the two calls.
    /// </summary>
    private static Url BuildQueryParams(
        Url url,
        string metadataPath,
        string session,
        string sessionIdentifier,
        string playbackSessionId,
        string playbackId,
        string token
    ) =>
        url.SetQueryParam("hasMDE", 1)
            .SetQueryParam("path", metadataPath)
            .SetQueryParam("mediaIndex", 0)
            .SetQueryParam("partIndex", 0)
            .SetQueryParam("protocol", "dash")
            .SetQueryParam("fastSeek", 1)
            .SetQueryParam("directPlay", 0) // Must be 0 for DASH streaming pipeline
            .SetQueryParam("directStream", 1) // 1 = copy streams without re-encoding (source quality)
            .SetQueryParam("subtitleSize", 100)
            .SetQueryParam("audioBoost", 100)
            .SetQueryParam("location", "lan") // lan bypasses remote bandwidth caps
            .SetQueryParam("maxVideoBitrate", 200000) // 200 Mbps = effectively unlimited
            .SetQueryParam("addDebugOverlay", 0)
            .SetQueryParam("autoAdjustQuality", 0)
            .SetQueryParam("directStreamAudio", 1) // 1 = copy audio without re-encoding
            .SetQueryParam("autoAdjustSubtitle", 1)
            .SetQueryParam("mediaBufferSize", 102400)
            .SetQueryParam("session", session)
            .SetQueryParam("subtitles", "none")
            .SetQueryParam("videoResolution", "3840x2160") // Request 4K/UHD output
            .SetQueryParam("videoQuality", 100) // Highest quality setting
            .SetQueryParam("Accept-Language", "en")
            .SetQueryParam("X-Plex-Session-Identifier", sessionIdentifier)
            .SetQueryParam("X-Plex-Client-Profile-Extra", CLIENT_PROFILE_EXTRA)
            .SetQueryParam("X-Plex-Incomplete-Segments", 1)
            .SetQueryParam("X-Plex-Product", "Plex Web")
            .SetQueryParam("X-Plex-Version", "4.158.0")
            .SetQueryParam("X-Plex-Client-Identifier", _clientIdentifier)
            .SetQueryParam("X-Plex-Platform", "Firefox")
            .SetQueryParam("X-Plex-Platform-Version", "147.0")
            .SetQueryParam("X-Plex-Features", "external-media,indirect-media,hub-style-list")
            .SetQueryParam("X-Plex-Model", "standalone")
            .SetQueryParam("X-Plex-Device", "Linux")
            .SetQueryParam("X-Plex-Device-Name", "Firefox")
            .SetQueryParam("X-Plex-Device-Screen-Resolution", "3840x2160,3840x2160") // Prevents downscaling
            .SetQueryParam("X-Plex-Token", token)
            .SetQueryParam("X-Plex-Language", "en")
            .SetQueryParam("X-Plex-Session-Id", playbackSessionId)
            .SetQueryParam("X-Plex-Playback-Session-Id", playbackSessionId)
            .SetQueryParam("X-Plex-Playback-Id", playbackId);

    /// <summary>
    /// Generates a 24-character alphanumeric session identifier.
    /// </summary>
    private static string GenerateSessionId() => Guid.NewGuid().ToString("N")[..24];

    /// <summary>
    /// Generates a stable client identifier for this Reaparr installation.
    /// </summary>
    private static string GenerateClientId() => $"{Guid.NewGuid():N}"[..25];
}
