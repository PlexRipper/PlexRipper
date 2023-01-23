using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class GetFolderPathByIdQuery : IRequest<Result<FolderPath>>
{
    public GetFolderPathByIdQuery(int id)
    {
        Id = id;
    }

    public int Id { get; }
}