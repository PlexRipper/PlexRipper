using FluentResults;
using MediatR;

namespace PlexRipper.Application.PlexLibraries
{
    public class DeleteMediaFromPlexLibraryCommand : IRequest<Result<bool>>
    {
        public DeleteMediaFromPlexLibraryCommand(int plexLibraryId)
        {
            PlexLibraryId = plexLibraryId;
        }

        public int PlexLibraryId { get; }
    }
}