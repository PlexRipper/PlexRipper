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
        RuleFor(x => x.DecisionRequest).NotNull();
        RuleFor(x => x.DecisionRequest.Path).NotEmpty();
        RuleFor(x => x.DecisionRequest.Path).Must(path => path?.Contains("/library/metadata/") == true);
        RuleFor(x => x.DecisionRequest.TranscodeSessionId).NotEmpty();
        RuleFor(x => x.DecisionRequest.XPlexSessionIdentifier).NotEmpty();
        RuleFor(x => x.DecisionRequest.ClientIdentifier).NotEmpty();
    }
}

public class GetDashTranscodeDecisionCommandHandler
    : ICommandHandler<GetDashTranscodeDecisionCommand, Result<GetDashTranscodeDecisionResult>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IPlexApiClientFactory _plexApiClientFactory;

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

        var decisionRequest = command.DecisionRequest;

        var debugDecisionUrl = new Url(connectionResult.Value.Url)
            .AppendPathSegment("video/:/transcode/universal/decision")
            .ApplyDashTranscodeQueryParams(decisionRequest, tokenResult.Value)
            .ToString();

        _log.Here().Information("Requesting Plex transcode decision URL: {Url}", debugDecisionUrl);

        var decisionResponse = await client.Transcoder.MakeDecisionAsync(decisionRequest).ToResponse();

        if (decisionResponse.IsFailed)
            return decisionResponse.ToResult();

        var mediaContainer = decisionResponse.Value.MediaContainerWithDecision?.MediaContainer;
        if (mediaContainer is null)
            return Result.Fail("Invalid decision response: missing MediaContainer").LogError();

        _log.Here().Debug("{@MediaContainer}", mediaContainer.ToString());
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
}
