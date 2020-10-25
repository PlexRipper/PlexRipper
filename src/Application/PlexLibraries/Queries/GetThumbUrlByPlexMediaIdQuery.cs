using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexLibraries.Queries
{
    public class GetThumbUrlByPlexMediaIdQuery : IRequest<Result<string>>
    {
        public GetThumbUrlByPlexMediaIdQuery(int mediaId, PlexMediaType plexMediaType)
        {
            MediaId = mediaId;
            PlexMediaType = plexMediaType;
        }

        public int MediaId { get; }

        public PlexMediaType PlexMediaType { get; }
    }
}