using FluentResults;
using MediatR;

namespace PlexRipper.Application
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