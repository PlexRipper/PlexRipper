using FluentResults;
using MediatR;

namespace PlexRipper.Application.PlexAuthentication.Queries
{
    public class GetPlexServerTokenQuery : IRequest<Result<string>>
    {
        public int PlexAccountId { get; }

        public int PlexServerId { get; }

        public GetPlexServerTokenQuery(int plexAccountId, int plexServerId)
        {
            PlexAccountId = plexAccountId;
            PlexServerId = plexServerId;
        }
    }
}