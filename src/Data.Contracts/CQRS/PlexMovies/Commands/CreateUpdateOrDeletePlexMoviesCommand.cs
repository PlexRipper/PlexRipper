using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class CreateUpdateOrDeletePlexMoviesCommand : IRequest<Result>
{
    public CreateUpdateOrDeletePlexMoviesCommand(PlexLibrary plexLibrary)
    {
        PlexLibrary = plexLibrary;
    }

    public PlexLibrary PlexLibrary { get; }
}