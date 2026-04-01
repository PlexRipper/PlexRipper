using Flurl;

namespace Reaparr.PlexApi;

public class GetTranscodeUrlCommandValidator : AbstractValidator<GetTranscodeUrlCommand>
{
    public GetTranscodeUrlCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.DownloadTaskKey).NotNull();
        RuleFor(x => x.DownloadTaskKey.IsValid).Equal(true);
        RuleFor(x => x.MetaDataPath)
            .NotEmpty()
            .Must(path => path.Contains("/library/metadata/"))
            .WithMessage("MetaDataPath must be in format '/library/metadata/{ratingKey}'");
    }
}

public class GetTranscodeUrlCommandHandler : ICommandHandler<GetTranscodeUrlCommand, Result<GetTranscodeUrlResult>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

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

        var playbackSessionId = Guid.NewGuid().ToString();
        var playbackId = Guid.NewGuid().ToString();

        var decisionRequest = new TranscodeDecisionRequest(command.MetaDataPath);

        var decisionResult = await _commandExecutor.Send(
            new GetDashTranscodeDecisionCommand(plexServerId, decisionRequest),
            cancellationToken
        );

        if (decisionResult.IsFailed)
            return decisionResult.ToResult().LogError();

        var decisionSummary = decisionResult.Value;

        var downloadUrl = new Url(plexServerConnection.Url)
            .AppendPathSegment("video/:/transcode/universal/start.mpd")
            .ApplyDashTranscodeQueryParams(decisionRequest.ToMakeDecisionRequest(), token)
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
                SuggestedClientType = decisionSummary.SuggestedClientType,
            }
        );
    }
}
