using System.Collections.Generic;
using FluentResults;
using MediatR;

namespace PlexRipper.Application.PlexDownloads
{
    public class DeleteDownloadTasksByIdCommand : IRequest<Result<bool>>
    {
        public List<int> DownloadTaskIds { get; }

        public DeleteDownloadTasksByIdCommand(List<int> downloadTaskIds)
        {
            DownloadTaskIds = downloadTaskIds;
        }
    }
}