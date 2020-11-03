using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexServers
{
    public class GetPlexServerByPlexTvShowEpisodeIdQuery : IRequest<Result<PlexServer>>
    {
        public GetPlexServerByPlexTvShowEpisodeIdQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }
}