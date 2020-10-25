using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexDownloads.Queries
{
    /// <summary>
    ///     Request all downloadTasks sorted in their respective <see cref="PlexServer" /> and <see cref="PlexLibrary" />
    /// </summary>
    public class GetDownloadTasksByPlexServerIdQuery : IRequest<Result<PlexServer>>
    {
        public GetDownloadTasksByPlexServerIdQuery(int plexServerId, bool includePlexAccount = false, bool includeServerStatus = false)
        {
            PlexServerId = plexServerId;
            IncludeServerStatus = includeServerStatus;
            IncludePlexAccount = includePlexAccount;
        }

        public int PlexServerId { get; }

        public bool IncludeServerStatus { get; }

        public bool IncludePlexAccount { get; }
    }
}