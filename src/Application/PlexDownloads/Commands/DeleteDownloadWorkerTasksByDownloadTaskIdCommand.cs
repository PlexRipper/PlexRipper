﻿using FluentResults;
using MediatR;

namespace PlexRipper.Application.PlexDownloads
{
    public class DeleteDownloadWorkerTasksByDownloadTaskIdCommand : IRequest<Result<bool>>
    {
        public int DownloadTaskId { get; }

        public DeleteDownloadWorkerTasksByDownloadTaskIdCommand(int downloadTaskId)
        {
            DownloadTaskId = downloadTaskId;
        }
    }
}