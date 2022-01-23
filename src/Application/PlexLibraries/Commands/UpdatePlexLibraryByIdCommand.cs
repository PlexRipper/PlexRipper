using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class UpdatePlexLibraryByIdCommand : IRequest<Result<bool>>
    {
        public UpdatePlexLibraryByIdCommand(PlexLibrary plexLibrary)
        {
            PlexLibrary = plexLibrary;
        }

        public PlexLibrary PlexLibrary { get; }
    }
}