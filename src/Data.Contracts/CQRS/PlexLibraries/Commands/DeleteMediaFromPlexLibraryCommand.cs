using FluentResults;
using MediatR;

namespace Data.Contracts;

public class DeleteMediaFromPlexLibraryCommand : IRequest<Result<bool>>
{
    public DeleteMediaFromPlexLibraryCommand(int plexLibraryId)
    {
        PlexLibraryId = plexLibraryId;
    }

    public int PlexLibraryId { get; }
}