using FluentResults;
using MediatR;

namespace PlexRipper.Application.PlexDownloads.Commands
{
    public class ClearCompletedDownloadTasksCommand : IRequest<Result<bool>> { }
}