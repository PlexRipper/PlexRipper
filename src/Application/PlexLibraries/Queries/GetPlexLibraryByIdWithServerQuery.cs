using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexLibraries
{
    public class GetPlexLibraryByIdWithServerQuery : IRequest<Result<PlexLibrary>>
    {
        public GetPlexLibraryByIdWithServerQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }
}