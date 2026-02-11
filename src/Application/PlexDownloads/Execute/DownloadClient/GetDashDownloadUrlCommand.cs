using System.Xml.Linq;
using FastEndpoints;
using FluentValidation;
using Flurl;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public record GetDashDownloadUrlCommand : ICommand<Result<string>>
{
    public required DownloadTaskKey DownloadTaskKey { get; init; }

    public required string MetaDataPath { get; init; }
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

public class GetDashDownloadUrlCommandHandler : ICommandHandler<GetDashDownloadUrlCommand, Result<string>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;

    // Stable client identifier per app install (generate once, reuse forever)
    private static readonly string _clientIdentifier = GenerateClientId();

    private const string CLIENT_PROFILE_EXTRA =
        "append-transcode-target-codec(type=videoProfile&context=streaming&videoCodec=h264%2Chevc&audioCodec=aac&protocol=dash)";

    public GetDashDownloadUrlCommandHandler(
        ILogger logger,
        IReaparrDbContext dbContext,
        IHttpClientFactory httpClientFactory
    )
    {
        _log = logger.ForContext<GetDashDownloadUrlCommandHandler>();
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Result<string>> ExecuteAsync(
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

        // Step 1: Call decision endpoint to initialize the transcoder session
        var decisionUrl = BuildQueryParams(
                plexServerConnection.Url.AppendPathSegment("video/:/transcode/universal/decision"),
                command.MetaDataPath,
                session,
                sessionIdentifier,
                playbackSessionId,
                playbackId,
                token
            )
            .ToString();

        _log.Here().Debug("Calling decision endpoint: {DecisionUrl}", decisionUrl);

        using var httpClient = _httpClientFactory.CreateClient();
        using var response = await httpClient.GetAsync(decisionUrl, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return Result
                .Fail($"Decision endpoint returned {(int)response.StatusCode}: {response.ReasonPhrase}")
                .LogError();
        }

        var decisionBody = await response.Content.ReadAsStringAsync(cancellationToken);

        _log.Here().Debug("Decision endpoint response: {DecisionBody}", decisionBody);

        // Validate decision response contains expected structure
        try
        {
            var doc = XDocument.Parse(decisionBody);
            var mediaContainer = doc.Root;
            if (mediaContainer?.Name != "MediaContainer")
            {
                return Result.Fail("Invalid decision response: missing MediaContainer").LogError();
            }

            // Log the decision code for debugging
            var mdeDecisionCode = mediaContainer.Attribute("mdeDecisionCode")?.Value ?? "unknown";
            var mdeDecisionText = mediaContainer.Attribute("mdeDecisionText")?.Value ?? "unknown";
            _log.Here()
                .Information(
                    "Decision returned: code={Code}, text={Text}, session={Session}, sessionId={SessionId}",
                    mdeDecisionCode,
                    mdeDecisionText,
                    session,
                    sessionIdentifier
                );
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError("Failed to parse decision response", ex)).LogError();
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

        return Result.Ok(downloadUrl);
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
            .SetQueryParam("directPlay", 0) // Force streaming mode (required for start.mpd)
            .SetQueryParam("directStream", 0) // Changed: 0 forces transcode, 1 attempts direct stream
            .SetQueryParam("subtitleSize", 100)
            .SetQueryParam("audioBoost", 100)
            .SetQueryParam("location", "wan") // Changed: wan allows remote transcoding
            .SetQueryParam("maxVideoBitrate", 20000) // High bitrate cap for best quality
            .SetQueryParam("addDebugOverlay", 0)
            .SetQueryParam("autoAdjustQuality", 0)
            .SetQueryParam("directStreamAudio", 0) // Allow audio transcode (TrueHD → AAC for DASH)
            .SetQueryParam("autoAdjustSubtitle", 1)
            .SetQueryParam("mediaBufferSize", 102400)
            .SetQueryParam("session", session)
            .SetQueryParam("subtitles", "burn")
            .SetQueryParam("Accept-Language", "en")
            .SetQueryParam("X-Plex-Session-Identifier", sessionIdentifier)
            .SetQueryParam("X-Plex-Client-Profile-Extra", CLIENT_PROFILE_EXTRA)
            .SetQueryParam("X-Plex-Incomplete-Segments", 1)
            .SetQueryParam("X-Plex-Product", "Plex Web")
            .SetQueryParam("X-Plex-Version", "4.157.0")
            .SetQueryParam("X-Plex-Client-Identifier", _clientIdentifier)
            .SetQueryParam("X-Plex-Platform", "Firefox")
            .SetQueryParam("X-Plex-Platform-Version", "147.0")
            .SetQueryParam("X-Plex-Features", "external-media,indirect-media,hub-style-list")
            .SetQueryParam("X-Plex-Model", "standalone")
            .SetQueryParam("X-Plex-Device", "Linux")
            .SetQueryParam("X-Plex-Device-Name", "Firefox") // TODO Change to a platform like nvidia shield with more codec capabilities?
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
