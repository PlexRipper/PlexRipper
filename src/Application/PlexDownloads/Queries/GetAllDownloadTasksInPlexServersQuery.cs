using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    /// <summary>
    ///     Request all downloadTasks sorted in their respective <see cref="PlexServer" /> and <see cref="PlexLibrary" />.
    ///     Includes the <see cref="DownloadWorkerTask"/>.
    /// </summary>
    public class GetAllDownloadTasksInPlexServersQuery : IRequest<Result<List<PlexServer>>>
    {
        public GetAllDownloadTasksInPlexServersQuery(bool includeServerStatus = false)
        {
            IncludeServerStatus = includeServerStatus;
        }

        public bool IncludeServerStatus { get; }
    }
}