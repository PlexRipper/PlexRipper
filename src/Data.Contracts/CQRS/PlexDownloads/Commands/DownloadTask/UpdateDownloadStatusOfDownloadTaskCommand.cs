﻿using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class UpdateDownloadStatusOfDownloadTaskCommand : IRequest<Result>
{
    public UpdateDownloadStatusOfDownloadTaskCommand(List<int> downloadTaskIds, DownloadStatus downloadStatus)
    {
        DownloadTaskIds = downloadTaskIds;
        DownloadStatus = downloadStatus;
    }

    public List<int> DownloadTaskIds { get; }

    public DownloadStatus DownloadStatus { get; }
}