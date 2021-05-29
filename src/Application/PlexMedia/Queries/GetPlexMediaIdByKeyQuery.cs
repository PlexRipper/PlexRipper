using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexMedia
{
    public class GetPlexMediaIdByKeyQuery : IRequest<Result<int>>
    {
        public int Key { get; }

        public PlexMediaType MediaType { get; }

        public int PlexServerId { get; }

        public GetPlexMediaIdByKeyQuery(int key, PlexMediaType mediaType, int plexServerId)
        {
            Key = key;
            MediaType = mediaType;
            PlexServerId = plexServerId;
        }
    }
}