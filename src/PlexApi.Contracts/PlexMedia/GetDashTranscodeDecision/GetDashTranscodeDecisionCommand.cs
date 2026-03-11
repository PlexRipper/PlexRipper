using FastEndpoints;
using LukeHagar.PlexAPI.SDK.Models.Requests;

namespace Reaparr.PlexApi.Contracts;

public record GetDashTranscodeDecisionCommand(int PlexServerId, MakeDecisionRequest DecisionRequest)
    : ICommand<Result<GetDashTranscodeDecisionResult>>;

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
