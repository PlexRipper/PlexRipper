using FastEndpoints;

namespace Reaparr.PlexApi.Contracts;

public record GetDashTranscodeDecisionCommand(
    int PlexServerId,
    string MetaDataPath,
    string ClientIdentifier,
    string Session,
    string SessionIdentifier,
    string PlaybackSessionId,
    string PlaybackId
) : ICommand<Result<GetDashTranscodeDecisionResult>>;

public record GetDashTranscodeDecisionResult
{
    public required string GeneralDecisionCode { get; init; }

    public required string GeneralDecisionText { get; init; }

    public required string TranscodeDecisionCode { get; init; }

    public required string TranscodeDecisionText { get; init; }

    public required string VideoDecision { get; init; }

    public required string AudioDecision { get; init; }

    public required VideoQuality TranscodedQuality { get; init; }
}
