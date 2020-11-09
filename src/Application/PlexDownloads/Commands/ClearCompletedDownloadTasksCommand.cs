using FluentResults;
using MediatR;

namespace PlexRipper.Application.PlexDownloads
{
    public class ClearCompletedDownloadTasksCommand : IRequest<Result<bool>> { }
}