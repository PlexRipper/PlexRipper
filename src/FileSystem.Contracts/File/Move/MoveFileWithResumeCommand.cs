using FastEndpoints;
using FluentResults;

namespace Reaparr.FileSystem.Contracts;

public record MoveFileWithResumeCommand : ICommand<Result>
{
    public required string SourcePath { get; init; }
    public required string TargetPath { get; init; }

    public required Action<MoveFileTransferProgressDTO> Progress { get; init; }
    public required long CurrentOffset { get; init; }
    public required long DataTotal { get; init; }
}
