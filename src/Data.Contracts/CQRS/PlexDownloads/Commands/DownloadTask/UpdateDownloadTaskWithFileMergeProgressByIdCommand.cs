using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class UpdateDownloadTaskWithFileMergeProgressByIdCommand : IRequest<Result>
{
    public FileMergeProgress FileMergeProgress { get; }

    public UpdateDownloadTaskWithFileMergeProgressByIdCommand(FileMergeProgress fileMergeProgress)
    {
        FileMergeProgress = fileMergeProgress;
    }
}