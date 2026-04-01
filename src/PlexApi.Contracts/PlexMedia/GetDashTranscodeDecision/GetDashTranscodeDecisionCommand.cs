namespace Reaparr.PlexApi.Contracts;

public record GetDashTranscodeDecisionCommand(int PlexServerId, TranscodeDecisionRequest DecisionRequest)
    : ICommand<Result<GetDashTranscodeDecisionResult>>;

public record GetDashTranscodeDecisionResult
{
    public required string GeneralDecisionCode { get; init; }

    public required string GeneralDecisionText { get; init; }

    public required string TranscodeDecisionCode { get; init; }

    public required string TranscodeDecisionText { get; init; }

    public required string VideoDecision { get; init; }

    public required string AudioDecision { get; init; }

    public required string PartDecision { get; init; }

    public required VideoQuality TranscodedQuality { get; init; }

    public required PlexDownloadClientType SuggestedClientType { get; init; }
}
