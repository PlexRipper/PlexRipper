using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class GetPlexTvShowByIdQuery : IRequest<Result<PlexTvShow>>
    {
        public GetPlexTvShowByIdQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }
}